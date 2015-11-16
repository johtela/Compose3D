namespace Compose3D.Geometry
{
	using Maths;
    using System;
    using Textures;
    using System.Collections.Generic;
    using System.Linq;

    public static class Solids
	{
		#region Edges

		private class Edge : IEquatable<Edge>
		{
			public readonly int Index1;
			public readonly	int Index2;

			public Edge (int index1, int index2)
			{
				Index1 = index1;
				Index2 = index2;
			}

			public override bool Equals (object obj)
			{
				return obj is Edge && Equals ((Edge)obj);
			}

			public override int GetHashCode ()
			{
				return Index1.GetHashCode () ^ Index2.GetHashCode ();
			}

			public override string ToString ()
			{
				return string.Format ("({0}, {1})", Index1, Index2);
			}

			public bool Equals (Edge other)
			{
				return (Index1 == other.Index1 && Index2 == other.Index2) ||
					(Index1 == other.Index2 && Index2 == other.Index1);
			}
		}

		#endregion

		#region Private helper functions

		private static int BoolToInt (bool b)
		{
			return b ? 1 : 0;
		}

		private static V SideVertex<V> (V vertex, Vec3 normal)
			where V : struct, IVertex
		{
			return VertexHelpers.New<V> (vertex.Position, normal);
		}

		private static IEnumerable<Edge> GetEdges<V> (Geometry<V> geometry)
			where V : struct, IVertex
		{
			var indices = geometry.Indices;
			for (int i = 0; i < indices.Length; i += 3)
			{
				yield return new Edge (indices [i], indices [i + 1]);
				yield return new Edge (indices [i + 1], indices [i + 2]);
				yield return new Edge (indices [i + 2], indices [i]);
			}
		}

		private static IEnumerable<Edge> DetermineOuterEdges (Edge[] edges)
		{
			var innerEdges = new HashSet<Edge> ();

			for (int i = 0; i < edges.Length; i++)
				for (int j = i + 1; j < edges.Length; j++)
					if (edges [i].Equals (edges [j]))
					{
						innerEdges.Add (edges [j]);
						break;
					}
			return edges.Except (innerEdges);
		}

		private static Vec3 CalculateNormal<V> (V[] vertices, V[] backVertices, int index1, int index2) 
			where V : struct, IVertex
		{
			return vertices [index1].Position.CalculateNormal (
				vertices [index2].Position, backVertices [index1].Position);
		}

		#endregion

		public static Geometry<V> Stretch<V> (this Geometry<V> frontFace, int repeatCount, 
			bool includeFrontFace, bool includeBackFace, IEnumerable<Mat4> transforms) 
			where V : struct, IVertex
		{
			var vertices = frontFace.Vertices;
			var zplane = vertices [0].Position.Z;
			if (!vertices.All (v => v.Position.Z == zplane))
				throw new ArgumentException ("Geometry is not on completely on the XY-plane.", "frontFace");

			var edges = GetEdges (frontFace).ToArray ();
			var outerEdges = DetermineOuterEdges (edges).ToArray ();
			var geometries = new Geometry<V> [(outerEdges.Length * repeatCount)
			                 + BoolToInt (includeFrontFace) + BoolToInt (includeBackFace)];
			var backFace = frontFace;
            var i = 0;
			var txenum = transforms.GetEnumerator ();
			if (includeFrontFace)
            	geometries[i++] = frontFace;

			for (var t = 0; t < repeatCount; t++)
            {
				if (!txenum.MoveNext ())
					throw new GeometryError ("Transforms exhausted prematurely.");
				backFace = frontFace.ManipulateVertices (v => true, 
					v => v.With (txenum.Current.Transform (v.Position), -v.Normal));
				var backVertices = backFace.Vertices;
                for (var j = 0; j < outerEdges.Length; i++, j++)
                {
                    var edge = outerEdges[j];
					var frontNormal = CalculateNormal (vertices, backVertices, edge.Index1, edge.Index2);
					var backNormal = CalculateNormal (backVertices, vertices, edge.Index2, edge.Index1);
					geometries[i] = Quadrilateral<V>.FromVertices (
						SideVertex (vertices[edge.Index2], frontNormal),
						SideVertex (vertices[edge.Index1], frontNormal),
						SideVertex (backFace.Vertices[edge.Index1], backNormal),
						SideVertex (backFace.Vertices[edge.Index2], backNormal));
                }
                vertices = backFace.Vertices;
            }
            if (includeBackFace)
				geometries[i++] = backFace.ReverseIndices ();
            return Composite.Create (geometries);
		}

		public static Geometry<V> Extrude<V>(this Geometry<V> frontFace, float depth, 
			bool includeBackFace) where V : struct, IVertex
		{
			return frontFace.Stretch (1, true, includeBackFace, 
				new Mat4[] { Mat.Translation<Mat4> (0f, 0f, -depth) });
		}

		public static Geometry<V> Extrude<V> (this Geometry<V> frontFace, float depth) 
			where V : struct, IVertex
		{
			return frontFace.Extrude (depth, true);
		}

		public static Geometry<V> Hollow<V> (this Geometry<V> frontFace, float scaleX, float scaleY) 
			where V : struct, IVertex
		{
			return frontFace.Center ().Stretch (1, false, false,  
				new Mat4[] { Mat.Scaling<Mat4> (scaleX, scaleY) }).Simplify ();
		}

		public static Geometry<V> Cube<V> (float width, float height, float depth) 
			where V : struct, IVertex
		{
			return Quadrilateral<V>.Rectangle (width, height).Extrude (depth);
		}
	}
}


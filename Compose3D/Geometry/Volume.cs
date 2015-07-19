namespace Compose3D.Geometry
{
	using System;
	using Arithmetics;
	using System.Collections.Generic;
	using System.Linq;

	public static class Volume
	{
		private struct Edge : IEquatable<Edge>
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

			public bool Equals (Edge other)
			{
				return (Index1 == other.Index1 && Index2 == other.Index2) ||
					(Index1 == other.Index2 && Index2 == other.Index1);
			}
		}

		private struct Triangle
		{
			public Edge[] Edges;

			public Triangle (params Edge[] edges)
			{
				Edges = edges;
			}


		}

		public static Geometry<V> Extrude<V> (this Geometry<V> frontFace, float depth)
			where V : struct, IVertex
		{
			var vertices = frontFace.Vertices.ToArray ();
			if (!vertices.All (v => v.Position.Z == 0f))
				throw new ArgumentException ("Geometry is not on completely on the XY-plane.", "frontFace");

			var backFace = frontFace.Transform (
				Mat.Translation<Mat4> (0f, 0f, depth) * Mat.Scaling<Mat4> (0f, 0f, -1f));

			var triangles = GetTriangles (frontFace).ToArray ();
			var edges = GetEdges (triangles).ToArray ();
			var outerEdges = OuterEdges (edges).ToArray ();

			for (int i = 0; i < outerEdges.Length; i++)
			{

			}

			return Composite.Create (frontFace, backFace);
		}

		private static IEnumerable<Triangle> GetTriangles<V> (Geometry<V> geometry)
			where V : struct, IVertex
		{
			var indices = geometry.Indices.ToArray ();
			for (int i = 0; i < indices.Length; i += 3)
			{
				yield return new Triangle (
					new Edge (indices [i], indices [i + 1]),
					new Edge (indices [i + 1], indices [i + 2]),
					new Edge (indices [i + 2], indices [i]));
			}
		}

		private static IEnumerable<Edge> GetEdges (Triangle[] triangles)
		{
			foreach (var triangle in triangles)
				for (int i = 0; i < 3; i++)
					yield return triangle.Edges [i];
		}

		private static IEnumerable<Edge> OuterEdges (Edge[] edges)
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
	}
}


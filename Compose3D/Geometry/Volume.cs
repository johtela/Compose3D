namespace Compose3D.Geometry
{
	using System;
	using Arithmetics;
	using System.Collections.Generic;
	using System.Linq;
	using OpenTK;

	public static class Volume
	{
		private class Edge : IEquatable<Edge>
		{
			public readonly Triangle Triangle;
			public readonly int Index1;
			public readonly	int Index2;

			public Edge (Triangle triangle, int index1, int index2)
			{
				Triangle = triangle;
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

		private class Triangle
		{
			public Edge[] Edges;

			public Triangle (int[] indices, int firstIndex)
			{
				Edges = new Edge[] 
				{
					new Edge (this, indices [firstIndex], indices [firstIndex + 1]),
					new Edge (this, indices [firstIndex + 1], indices [firstIndex + 2]),
					new Edge (this, indices [firstIndex + 2], indices [firstIndex])
				};
			}

			public int ThirdIndex (Edge edge)
			{
				var other = Edges.First (e => !e.Equals (edge));
				return edge.Index1 != other.Index1 && edge.Index2 != other.Index1 ?
					other.Index1 : other.Index2;
			}
		}

		public static Geometry<V> Extrude<V> (this Geometry<V> frontFace, float depth)
			where V : struct, IVertex
		{
			var vertices = frontFace.Vertices;
			if (!vertices.All (v => v.Position.Z == 0f))
				throw new ArgumentException ("Geometry is not on completely on the XY-plane.", "frontFace");

			var backFace = frontFace.Transform (
				Mat.Translation<Mat4> (0f, 0f, depth) * Mat.Scaling<Mat4> (1f, 1f, -1f));
			var edges = GetEdges (GetTriangles (frontFace)).ToArray ();
			var outerEdges = DetermineOuterEdges (edges).ToArray ();

			var geometries = new Geometry<V> [outerEdges.Length + 2];
			geometries [0] = frontFace.ReverseIndices ();
			geometries [1] = backFace;

			for (int i = 0; i < outerEdges.Length; i++)
			{
				var edge = outerEdges [i];
				var outerPos = vertices [edge.Index1].Position;
				var normal = Mat.RotationZ<Mat3> (MathHelper.PiOver2) * (vertices [edge.Index2].Position - outerPos);
				var innerPos = vertices [edge.Triangle.ThirdIndex (edge)].Position;
				if (normal.Dot (outerPos - innerPos) < 0f)
					normal = -normal;
				geometries [i + 2] = Quadrilateral<V>.FromVertices (frontFace.Material, 
					ChangeNormal (vertices [edge.Index1], normal),
					ChangeNormal (vertices [edge.Index2], normal),
					ChangeNormal (backFace.Vertices [edge.Index2], normal),
					ChangeNormal (backFace.Vertices [edge.Index1], normal));
			}
			return Composite.Create (geometries);
		}

		private static V ChangeNormal<V> (V vertex, Vec3 normal)
			where V : struct, IVertex
		{
			return new V () 
			{ 
				Position = vertex.Position,
				Color = vertex.Color,
				Normal = normal
			};
		}

		private static IEnumerable<Triangle> GetTriangles<V> (Geometry<V> geometry)
			where V : struct, IVertex
		{
			var indices = geometry.Indices;
			for (int i = 0; i < indices.Length; i += 3)
				yield return new Triangle (indices, i);
		}

		private static IEnumerable<Edge> GetEdges (IEnumerable<Triangle> triangles)
		{
			foreach (var triangle in triangles)
				for (int i = 0; i < 3; i++)
					yield return triangle.Edges [i];
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

		public static Geometry<V> Cube<V> (float width, float height, float depth, IMaterial material) where V : struct, IVertex
		{
			return Quadrilateral<V>.Rectangle (width, height, material).Extrude (depth);
		}
	}
}


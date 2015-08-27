﻿namespace Compose3D.Geometry
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

		public static Geometry<V> Extrude<V> (this Geometry<V> frontFace, float depth)
			where V : struct, IVertex
		{
			var vertices = frontFace.Vertices;
			if (!vertices.All (v => v.Position.Z == 0f))
				throw new ArgumentException ("Geometry is not on completely on the XY-plane.", "frontFace");

			var backFace = frontFace.Transform (
				Mat.Translation<Mat4> (0f, 0f, -depth) * Mat.Scaling<Mat4> (1f, 1f, -1f));
			var edges = GetEdges (frontFace).ToArray ();
			var outerEdges = DetermineOuterEdges (edges).ToArray ();

			var geometries = new Geometry<V> [outerEdges.Length + 2];
			geometries [0] = frontFace;
			geometries [1] = backFace.ReverseIndices ();

			for (int i = 0; i < outerEdges.Length; i++)
			{
				var edge = outerEdges [i];
				var normal = Mat.RotationZ<Mat3> (MathHelper.PiOver2) * 
					(vertices [edge.Index2].Position - vertices [edge.Index1].Position);
				geometries [i + 2] = Quadrilateral<V>.FromVertices (frontFace.Material, 
					ChangeNormal (vertices [edge.Index2], normal),
					ChangeNormal (vertices [edge.Index1], normal),
					ChangeNormal (backFace.Vertices [edge.Index1], normal),
					ChangeNormal (backFace.Vertices [edge.Index2], normal));
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

		public static Geometry<V> Cube<V> (float width, float height, float depth, IMaterial material) where V : struct, IVertex
		{
			return Quadrilateral<V>.Rectangle (width, height, material).Extrude (depth);
		}
	}
}


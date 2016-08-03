namespace Compose3D.Geometry
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using OpenTK.Graphics.OpenGL4;
	using Extensions;
	using Maths;
	using Textures;

	public class Edge : IEquatable<Edge>
	{
		public readonly int Index1;
		public readonly int Index2;

		public Edge (int index1, int index2)
		{
			Index1 = index1;
			Index2 = index2;
		}

		public Edge Reversed
		{
			get { return new Edge (Index2, Index1); }
		}

		public Edge RelativeTo (int index)
		{
			return Index1 == index ? this : Reversed;
		}

		public bool Contains (int index)
		{
			return Index1 == index || Index2 == index;
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

	public static class EdgeHelpers
	{
		public static IEnumerable<Edge> GetEdges<V> (this Geometry<V> geometry, BeginMode primitive)
			where V : struct, IVertex
		{
			switch (primitive)
			{
				case BeginMode.Triangles:
					return GetTrianglesEdges (geometry.Indices, 3);
				case BeginMode.TriangleStrip:
					return GetTrianglesEdges (geometry.Indices, 1);
				default:
					throw new ArgumentException ("Unsupported primitive type: " + primitive,
						nameof (primitive));
			}
		}

		private static IEnumerable<Edge> GetTrianglesEdges (int[] indices, int increment)
		{
			for (int i = 2; i < indices.Length; i+= increment)
			{
				yield return new Edge (indices[i - 2], indices[i - 1]);
				yield return new Edge (indices[i - 1], indices[i]);
				yield return new Edge (indices[i], indices[i - 2]);
			}
		}

		public static IEnumerable<Edge> GetEdges<P, V> (this Path<P, V> path)
			where P : struct, IPositional<V>
			where V : struct, IVec<V, float>
		{
			return Enumerable.Range (1, path.Nodes.Length - 1).Select (i => new Edge (i - 1, i));
		}
	}
}

namespace Compose3D.Geometry
{
	using System;
	using Arithmetics;
	using System.Collections.Generic;
	using System.Linq;

	public static class Volume
	{
		private struct Edge
		{
			public readonly int Index1;
			public readonly	int Index2;

			public Edge (int index1, int index2)
			{
				Index1 = index1;
				Index2 = index2;
			}

			public bool IsOpposingEdge (Edge other)
			{
				return Index1 == other.Index2 && Index2 == other.Index1;
			}
		}

		public static Geometry<V> Extrude<V> (this Geometry<V> frontFace, float depth)
			where V : struct, IVertex
		{
			if (!Is2DGeometry (frontFace))
				throw new ArgumentException ("Geometry is not on completely on the 2D plane.", "frontFace");
			var backFace = frontFace.Transform (
				Mat.Translation<Mat4> (0f, 0f, depth) * Mat.Scaling<Mat4> (0f, 0f, -1f));
			return Composite.Create (frontFace, backFace);
		}

		private static bool Is2DGeometry<V> (Geometry<V> geometry)
			where V : struct, IVertex
		{
			return geometry.Vertices.All (v => v.Position.Z == 0f);
		}

		private static IEnumerable<Edge> GetEdges<V> (Geometry<V> geometry)
			where V : struct, IVertex
		{
			var indices = geometry.Indices.ToArray ();
			for (int i = 0; i < indices.Length; i += 3)
			{
				yield return new Edge (indices [i], indices [i + 1]);
				yield return new Edge (indices [i + 1], indices [i + 2]);
				yield return new Edge (indices [i + 2], indices [i]);
			}
		}

		private static IEnumerable<Edge> RemoveInnerEdges (IEnumerable<Edge> edges)
		{
			var edgeList = edges.ToList ();
			var removed = new HashSet<Edge> ();

			for (int i = 0; i < edgeList.Count; i++)
				for (int j = i + 1; j < edgeList.Count; j++) 
					if (edgeList [i].IsOpposingEdge (edgeList [j]))
					{
						removed.Add (edgeList [i]);
						removed.Add (edgeList [j]);
					}
			return edges.Except (removed);
		}
	}
}


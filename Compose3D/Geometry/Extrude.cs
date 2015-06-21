namespace Compose3D.Geometry
{
	using System;
	using Arithmetics;
	using System.Collections.Generic;
	using System.Linq;

	public class Extrude<V> : Geometry<V> where V : struct, IVertex
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

		public readonly Geometry<V> Geometry2D;

		public Extrude (Geometry<V> geometry2D)
		{
			if (!Is2DGeometry (geometry2D))
				throw new ArgumentException ("Geometry is not on completely on the 2D plane.", "geometry2D");
			Geometry2D = geometry2D;
		}

		private static bool Is2DGeometry (Geometry<V> geometry)
		{
			return geometry.Vertices.All (v => v.Position.Z == 0f);
		}

		private static IEnumerable<Edge> GetEdges (Geometry<V> geometry)
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

		#region implemented abstract members of Geometry

		public override int VertexCount
		{
			get
			{
				throw new NotImplementedException ();
			}
		}

		public override IEnumerable<V> Vertices
		{
			get
			{
				throw new NotImplementedException ();
			}
		}

		public override IEnumerable<int> Indices
		{
			get
			{
				throw new NotImplementedException ();
			}
		}

		#endregion
	}
}


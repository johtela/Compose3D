namespace Compose3D.Geometry
{
	using System;
	using System.Collections.Generic;
	using Maths;
	using Extensions;

	internal class VertexFilter<V> : Wrapper<V>
		where V : struct, IVertex3D
	{
		private Dictionary<int, int> _relocationTable;
		private Func<V, bool> _predicate;
		
		public VertexFilter (Geometry<V> geometry, Func<V, bool> predicate) :
			base (geometry)
		{
			_relocationTable = new Dictionary<int, int> ();
			_predicate = predicate;
		}
		
		protected override IEnumerable<V> GenerateVertices ()
		{
			var currInd = 0;
			for (int i = 0; i < _geometry.Vertices.Length; i++)
			{
				var vert = _geometry.Vertices [i];
				if (_predicate (vert))
				{
					_relocationTable.Add (i, currInd++);
					yield return vert;
				}
			}
		}
		
		protected override IEnumerable<int> GenerateIndices ()
		{
			var indices = _geometry.Indices;
            for (int i = 0; i < indices.Length; i += 3)
            {
                if (_relocationTable.TryGetValue (indices[i], out int v1) &&
                    _relocationTable.TryGetValue (indices[i + 1], out int v2) &&
                    _relocationTable.TryGetValue (indices[i + 2], out int v3))
                {
                    yield return v1;
                    yield return v2;
                    yield return v3;
                }
            }
        }
	}
	
	public static class VertexFilters
	{
		public static Geometry<V> FilterVertices<V> (this Geometry<V> geometry, Func<V, bool> predicate)
			where V: struct, IVertex3D
		{
			return new VertexFilter<V> (geometry, predicate);
		}
		
		public static bool Facing<V> (this V vertex, Vec3 direction)
			where V : struct, IVertex3D
		{
			return vertex.normal.Dot (direction).ApproxEquals (1f, 0.1f);
		}
	}
}
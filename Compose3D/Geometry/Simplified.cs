namespace Compose3D.Geometry
{
	using Compose3D.Maths;
	using System.Collections.Generic;
	using System.Linq;

	internal class Simplified<V> : Wrapper<V> where V : struct, IVertex
	{
		private Octree<V, int, Vec3> _octree;

		public Simplified (Geometry<V> geometry)
			: base (geometry)
		{
			_octree = new Octree<V, int, Vec3> ();
		}

		protected override IEnumerable<V> GenerateVertices ()
		{
			var i = 0;
			foreach (var vert in _geometry.Vertices)
			{
				if (_octree.Add (vert, i))
				{
					i++;
					yield return vert;
				}
			}
		}

		protected override IEnumerable<int> GenerateIndices ()
		{
			var vertices = Vertices;
			foreach (var ind in _geometry.Indices)
				yield return _octree [_geometry.Vertices [ind]];
		}
	}

	public static class Simplified
	{
		public static Geometry<V> Simplify<V> (this Geometry<V> geometry)
			where V : struct, IVertex
		{
			return new Simplified<V> (geometry);
		}
	}
}


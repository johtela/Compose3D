namespace Compose3D.Geometry
{
	using System;
	using Compose3D.Maths;
	using System.Collections.Generic;
	using System.Linq;

	internal class Simplified<V> : Wrapper<V> where V : struct, IVertex
	{
		private Octree<V, int, Vec3> _octree;
		private float _minSmoothAngle;

		public Simplified (Geometry<V> geometry, float minSmoothAngle)
			: base (geometry)
		{
			_octree = new Octree<V, int, Vec3> ();
			_minSmoothAngle = minSmoothAngle;
		}

		private IEnumerable<V> Smoothen (V[] reduced, int count)
		{
			var result = new V[count];
			var resCnt = 0;
			for (int i = 0; i < count; i++)
			{
				var vert = reduced [i];
				var normal = vert.Normal;
				if (normal != new Vec3 (0f))
				{
					var duplicates = (from dupl in _octree.FindByPosition (vert.Position)
					                  where !dupl.Equals (vert)
					                  select dupl);
					
					foreach (var dupl in duplicates)
					{
						var dnorm = dupl.Item1.Normal;
						if (dnorm.Dot (normal.Normalized) >= _minSmoothAngle)
						{
							normal += dnorm;
							_octree [dupl.Item1] = resCnt;
							reduced [dupl.Item2].Normal = new Vec3 (0f);
						}
						else
						{
							
						}
					}
					vert.Normal = normal.Normalized;
					result [resCnt++] = vert;
				}
			}
			return result.Take (resCnt);
		}

		protected override IEnumerable<V> GenerateVertices ()
		{
			var cnt = 0;
			var reduced = new V[_geometry.Vertices.Length];
			foreach (var vert in _geometry.Vertices)
			{
				if (_octree.Add (vert, cnt))
					reduced [cnt++] = vert;
			}
			return _minSmoothAngle == 0f ? 
				reduced.Take (cnt) :
				Smoothen (reduced, cnt);
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
			return new Simplified<V> (geometry, 0f);
		}

		public static Geometry<V> Smoothen<V> (this Geometry<V> geometry, float minSmoothAngle)
			where V : struct, IVertex
		{
			return new Simplified<V> (geometry, minSmoothAngle);
		}
	}
}


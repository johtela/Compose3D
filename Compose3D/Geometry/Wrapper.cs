namespace Compose3D.Geometry
{
	using System.Collections.Generic;

	internal class Wrapper<V> : Geometry<V> where V : struct, IVertex3D
	{
		protected internal Geometry<V> _geometry;

		public Wrapper (Geometry<V> geometry)
		{
			_geometry = geometry;
		}

		protected override IEnumerable<V> GenerateVertices ()
		{
			return _geometry.Vertices;
		}

		protected override IEnumerable<int> GenerateIndices ()
		{
			return _geometry.Indices;
		}
	}
}


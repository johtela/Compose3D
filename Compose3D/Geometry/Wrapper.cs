﻿namespace Compose3D.Geometry
{
	using System.Collections.Generic;
	using System.Linq;

	internal class Wrapper<V> : Geometry<V> where V : struct, IVertex
	{
		protected Geometry<V> _geometry;

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

		public override IMaterial Material
		{
			get { return _geometry.Material; }
		}
	}
}

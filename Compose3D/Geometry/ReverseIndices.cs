namespace Compose3D.Geometry
{
	using System.Collections.Generic;
	using System.Linq;

	internal class ReverseIndices<V> : Wrapper<V> where V : struct, IVertex3D
	{
		public ReverseIndices (Geometry<V> geometry)
			: base (geometry)
		{ }

		protected override IEnumerable<int> GenerateIndices ()
		{
			return _geometry.Indices.Reverse ();
		}
	}
}


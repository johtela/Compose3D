namespace Compose3D.Geometry
{
    using Compose3D.Maths;
    using System.Collections.Generic;
    using System.Linq;

	/// <summary>
	/// Basic building block for composite geometries.
	/// </summary>
	internal class Composite<V> : Geometry<V> where V : struct, IVertex3D
	{
		private Geometry<V>[] _geometries;

		public Composite (params Geometry<V>[] geometries)
		{
			_geometries = geometries;
		}

		public Composite (IEnumerable<Geometry<V>> geometries)
		{
			_geometries = geometries.ToArray ();
		}

		protected override IEnumerable<V> GenerateVertices ()
		{
			return _geometries.SelectMany (g => g.Vertices);
		}

		protected override IEnumerable<int> GenerateIndices ()
		{
			for (int g = 0, c = 0; g < _geometries.Length; g++)
			{
				foreach (var i in _geometries[g].Indices)
					yield return c + i;
				c += _geometries [g].Vertices.Length;
			}
		}
	}

	/// <summary>
	/// Helper methods to create various composite geometries.
	/// </summary>
	public static class Composite
	{
		public static Geometry<V> Create<V> (params Geometry<V>[] geometries) where V : struct, IVertex3D
		{
			return new Composite<V> (geometries);
		}

		public static Geometry<V> Create<V> (IEnumerable<Geometry<V>> geometries) where V : struct, IVertex3D
		{
			return new Composite<V> (geometries);
		}
	}
}

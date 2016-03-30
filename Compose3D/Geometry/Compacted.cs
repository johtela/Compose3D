namespace Compose3D.Geometry
{
	using System.Collections.Generic;

	internal class Compacted<V> : Geometry<V> where V : struct, IVertex
	{
		private V[] _vertices;
		private int[] _indices;

		public Compacted (Geometry<V> geometry)
		{
			_vertices = geometry.Vertices;
			_indices = geometry.Indices;
		}

		protected override IEnumerable<V> GenerateVertices ()
		{
			return _vertices;
		}

		protected override IEnumerable<int> GenerateIndices ()
		{
			return _indices;
		}
	}

	public static class Compacted
	{
		public static Geometry<V> Compact<V> (this Geometry<V> geometry)
			where V : struct, IVertex
		{
			return new Compacted<V> (geometry);
		}
	}
}


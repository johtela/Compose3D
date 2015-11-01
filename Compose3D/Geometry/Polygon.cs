namespace Compose3D.Geometry
{
	using Arithmetics;
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public class Polygon<V> : Primitive<V> where V : struct, IVertex
	{
		private int[] _indices;

		public Polygon (V[] vertices) : base (vertices)
		{
			_indices = Enumerable.Range (0, vertices.Length).ToArray ();
		}

		protected override IEnumerable<int> GenerateIndices ()
		{
			return _indices;
		}
	}
}


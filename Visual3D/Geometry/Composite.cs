namespace Visual3D
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using OpenTK;
	using OpenTK.Graphics.OpenGL;

	internal class Composite<V> : Geometry<V> where V : struct, IVertex
	{
		private Geometry<V>[] _geometries;
		private int? _vertexCount;

		public Composite (params Geometry<V>[] geometries)
		{
			_geometries = geometries;
		}

		public override int VertexCount
		{
			get
			{
				if (!_vertexCount.HasValue)
					_vertexCount = _geometries.Sum (g => g.VertexCount);
				return _vertexCount.Value;
			}
		}

		public override IEnumerable<V> Vertices
		{
			get { return _geometries.SelectMany (g => g.Vertices); }
		}

		public override IEnumerable<int> Indices
		{
			get
			{
				for (int g = 0, c = 0; g < _geometries.Length; g++)
				{
					foreach (var i in _geometries[g].Indices)
						yield return c + i;
					c += _geometries[g].VertexCount;
				}
			}
		}
	}
}

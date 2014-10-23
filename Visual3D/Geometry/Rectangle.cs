namespace Visual3D
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using OpenTK;
	using OpenTK.Graphics.OpenGL;

	internal class Rectangle<V> : Geometry<V> where V : struct, IVertex
	{
		private Vector2 _size;
		private Vector4 _color;
		private V[] _vertices;
		private int[] _indices;

		public Rectangle (float width, float height, Vector4 color)
		{
			_size = new Vector2 (width, height);
			_color = color;
		}

		public override int VertexCount
		{
			get { return 4; }
		}

		public override IEnumerable<V> Vertices
		{
			get
			{
				if (_vertices == null)
				{
					var right = _size.X / 2.0f;
					var top = _size.Y / 2.0f;
					_vertices = new V[] {
						Vertex (new Vector4 (right, top, 0.0f, 1.0f), _color),
						Vertex (new Vector4 (right, -top, 0.0f, 1.0f), _color),
						Vertex (new Vector4 (-right, -top, 0.0f, 1.0f), _color),
						Vertex (new Vector4 (-right, top, 0.0f, 1.0f), _color)
					};
				}
				return _vertices;
			}
		}

		public override IEnumerable<int> Indices
		{
			get
			{
				if (_indices == null)
					_indices = new int[] { 0, 1, 2, 2, 3, 0 };
				return _indices;
			}
		}
	}
}

namespace Compose3D.SceneGraph
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using OpenTK.Graphics.OpenGL;
	using GLTypes;
	using Geometry;
	using Maths;
	using Textures;

	public class Window<V> : SceneNode 
		where V : struct, IVertex
	{
		public Texture Texture { get; private set; }
		public Vec2 Position { get; set; }
		public Vec2 Size { get; set; }

		private Quadrilateral<V> _rectangle;
		private VBO<V> _vertexBuffer;
		private VBO<int> _indexBuffer;

		public Window (SceneGraph graph, Texture texture, Vec2 position, Vec2 size)
			: base (graph)
		{
			Texture = texture;
			Position = position;
			Size = size;
			_rectangle = Quadrilateral<V>.Rectangle (size.X, size.Y);
		}

		public VBO<V> VertexBuffer
		{
			get
			{
				if (_vertexBuffer == null)
					_vertexBuffer = new VBO<V> (_rectangle.Vertices, BufferTarget.ArrayBuffer);
				return _vertexBuffer;
			}
		}

		public VBO<int> IndexBuffer
		{
			get
			{
				if (_indexBuffer == null)
					_indexBuffer = new VBO<int> (_rectangle.Indices, BufferTarget.ElementArrayBuffer);
				return _indexBuffer;
			}
		}
	}
}

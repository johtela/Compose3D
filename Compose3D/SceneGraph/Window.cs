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
		where V : struct, IVertex, ITextured
	{
		public Texture Texture { get; private set; }

		private Geometry<V> _rectangle;
		private VBO<V> _vertexBuffer;
		private VBO<int> _indexBuffer;

		public Window (SceneGraph graph, Texture texture)
			: base (graph)
		{
			Texture = texture;
			_rectangle = Quadrilateral<V>.Rectangle (1f, 1f).Translate (0.5f, -0.5f);
			_rectangle.ApplyTextureFront (1f, new Vec2 (0f), new Vec2 (1f));
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
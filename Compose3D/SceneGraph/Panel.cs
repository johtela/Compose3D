namespace Compose3D.SceneGraph
{
	using System;
	using System.Drawing;
	using OpenTK;
	using OpenTK.Graphics.OpenGL;
	using GLTypes;
	using Geometry;
	using Maths;
	using Reactive;
	using Textures;
	using DataStructures;
	using Extensions;

	public class Panel<V> : SceneNode 
		where V : struct, IVertex, ITextured
	{
		public Texture Texture { get; set; }

		private Geometry<V> _rectangle;
		private VBO<V> _vertexBuffer;
		private VBO<int> _indexBuffer;

		public Panel (SceneGraph graph, bool flipVertically)
			: base (graph)
		{
			_rectangle = Quadrilateral<V>.Rectangle (1f, 1f).Translate (0.5f, -0.5f);
			if (flipVertically)
				_rectangle.ApplyTextureFront (1f, new Vec2 (0f, 1f), new Vec2 (1f, 0f));
			else
				_rectangle.ApplyTextureFront (1f, new Vec2 (0f), new Vec2 (1f));
		}

		public Panel (SceneGraph graph, bool flipVertically, Texture texture)
			: this (graph, flipVertically)
		{
			Texture = texture;
		}

		public Mat4 GetModelViewMatrix (Vec2 viewportSize)
		{
			var texSize = Texture.Size * 2;
			var scalingMat = Mat.Scaling<Mat4> (texSize.X / viewportSize.X, texSize.Y / viewportSize.Y);
			return Transform * scalingMat;
		}

		public Aabb<Vec2> GetBoundsOnScreen (Vec2 viewportSize)
		{
			var halfSize = viewportSize / 2f;
			var toScreen = Mat.Scaling<Mat4> (halfSize.X, halfSize.Y) * 
				Mat.Translation<Mat4> (0f, 1f) *
				GetModelViewMatrix (viewportSize);
			var bbox = toScreen * _rectangle.BoundingBox;
			return new Aabb<Vec2> (new Vec2 (bbox.Min), new Vec2 (bbox.Max));
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
	
	public static class PanelReactions
	{
		public static Reaction<Vec2> WhenMouseOn<V> (this Reaction<Vec2> reaction, 
			Panel<V> panel, GameWindow window) where V : struct, IVertex, ITextured
		{
			return reaction.Where (mpos =>
			{
				var vportSize = new Vec2 (window.ClientSize.Width, window.ClientSize.Height);
				var bbox = panel.GetBoundsOnScreen (vportSize);
				var pos = new Vec2 (mpos.X, vportSize.Y - mpos.Y);
				return bbox & pos;
			});
		}
	}
}
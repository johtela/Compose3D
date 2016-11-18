namespace Compose3D.SceneGraph
{
	using System;
	using System.Linq;
	using OpenTK;
	using OpenTK.Input;
	using OpenTK.Graphics.OpenGL4;
	using DataStructures;
	using Geometry;
	using GLTypes;
	using Maths;
	using Textures;

	public enum UpdateAction { Done, Redraw, HandleInput };

	public class Panel<V> : SceneNode 
		where V : struct, IVertex, ITextured
	{
		public Texture Texture { get; set; }

		private Geometry<V> _rectangle;
		private VBO<V> _vertexBuffer;
		private VBO<int> _indexBuffer;
		private bool _flipVertically;
		private bool _movable;
		private bool _moving;
		private bool _resizing;
		private Vec3 _origOffs;
		private Vec2i _origMousePos;
		private Vec2i _origSize;

		public Panel (SceneGraph graph, bool flipVertically, bool movable, Vec2i repeat)
			: base (graph)
		{
			_rectangle = Quadrilateral<V>.Rectangle (1f, 1f).Translate (0.5f, -0.5f);
			_flipVertically = flipVertically;
			_movable = movable;
			var fac = repeat.Convert<Vec2i, Vec2> ();
			if (flipVertically)
				_rectangle.ApplyTextureFront (1f, TexturePos.TopLeft * fac, TexturePos.BottomRight * fac);
			else
				_rectangle.ApplyTextureFront (1f, TexturePos.BottomLeft, TexturePos.TopRight * fac);
		}

		public Panel (SceneGraph graph, bool flipVertically, bool movable, Vec2i repeat, Texture texture)
			: this (graph, flipVertically, movable, repeat)
		{
			Texture = texture;
		}

		public Panel (SceneGraph graph, bool flipVertically, bool movable, Texture texture)
			: this (graph, flipVertically, movable, new Vec2i (1), texture) { }

		public Mat4 GetModelViewMatrix (Vec2i viewportSize)
		{
			var texSize = Texture.Size * 2;
			var scalingMat = Mat.Scaling<Mat4> (
				(float)texSize.X / viewportSize.X, 
				(float)texSize.Y / viewportSize.Y);
			return Transform * scalingMat;
		}

		public Aabb<Vec2> GetBoundsOnScreen (Vec2i viewportSize)
		{
			var halfSize = viewportSize / 2;
			var toScreen = Mat.Scaling<Mat4> (halfSize.X, halfSize.Y) * 
				Mat.Translation<Mat4> (1f, 1f) *
				GetModelViewMatrix (viewportSize);
			var bbox = toScreen * _rectangle.BoundingBox;
			return new Aabb<Vec2> (new Vec2 (bbox.Min), new Vec2 (bbox.Max));
		}

		public bool MouseOnPanel (Vec2i mousePos, Vec2i vportSize)
		{
			var bbox = GetBoundsOnScreen (vportSize);
			var pos = new Vec2 (mousePos.X, vportSize.Y - mousePos.Y);
			return bbox & pos;
		}

		public Vec2i PanelCoordinatesAtMousePos (Vec2i mousePos, Vec2i vportSize)
		{
			var bbox = GetBoundsOnScreen (vportSize);
			var pos = new Vec2 (mousePos.X, vportSize.Y - mousePos.Y);
			return bbox & pos ? 
				new Vec2i (
					(int)(pos.X - bbox.Left), 
					_flipVertically ? 
						(int)(bbox.Top - pos.Y) : 
						(int)(pos.Y - bbox.Bottom)) :
				new Vec2i (-1);
		}

		public virtual UpdateAction Update (Vec2i viewportSize, MouseDevice mouse)
		{
			if (_movable &&
				InputState.MouseButtonPressed (MouseButton.Left) &&
				(InputState.KeyDown (Key.LControl) || InputState.KeyDown (Key.RControl)) &&
				MouseOnPanel (new Vec2i (mouse.X, mouse.Y), viewportSize))
			{
				_origMousePos = new Vec2i (mouse.X, mouse.Y);
				var parent = Parent as TransformNode;
				if (parent == null)
					throw new InvalidOperationException (
						"Parent node of movable panel needs to be TransformNode. " +
						"Alternatively you can create the panel with 'movable' parameter as false.");
				_origOffs = parent.Offset;
				_moving = true;
				return UpdateAction.Done;
			}
			else if (_moving && InputState.MouseButtonDown (MouseButton.Left))
			{
				var delta = new Vec2i (mouse.X, mouse.Y) - _origMousePos;
				if (delta != default(Vec2i))
				{
					var vport = new Vec2 (viewportSize.X, -viewportSize.Y);
					var normalizedDelta = new Vec2 (delta.X, delta.Y) * 2f / vport;
					((TransformNode)Parent).Offset = _origOffs + new Vec3(normalizedDelta, 0f);
				}
				return UpdateAction.Done;
			}
			if (InputState.MouseButtonPressed (MouseButton.Right) &&
				(InputState.KeyDown (Key.LControl) || InputState.KeyDown (Key.RControl)) &&
				MouseOnPanel (new Vec2i (mouse.X, mouse.Y), viewportSize))
			{
				_origMousePos = new Vec2i (mouse.X, mouse.Y);
				_origSize = GetSize ();
				_resizing = true;
				return UpdateAction.Done;
			}
			else if (_resizing && InputState.MouseButtonDown (MouseButton.Right))
			{
				var delta = new Vec2i (mouse.X, mouse.Y) - _origMousePos;
				if (delta != default (Vec2i))
					Resize (_origSize + delta);
				return UpdateAction.Redraw;
			}
			_moving = false;
			_resizing = false;
			return UpdateAction.HandleInput;
		}

		protected virtual Vec2i GetSize ()
		{
			return Texture.Size;
		}

		protected virtual void Resize (Vec2i size) { }

		public static void UpdateAll (SceneGraph sceneGraph, GameWindow window, Vec2i viewportSize)
		{
			InputState.Update (window);
			var panels = sceneGraph.Root.Traverse ().OfType<Panel<V>> ();
			foreach (var panel in panels)
				panel.Update (viewportSize, window.Mouse);
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

		public static SceneNode Movable (SceneGraph graph, bool flipVertically, Texture texture, Vec2 pos)
		{
			return new Panel<V> (graph, flipVertically, true, texture)
				.Offset (new Vec3 (pos, 0f));
		}

		public static SceneNode Movable (SceneGraph graph, bool flipVertically, Texture texture, Vec2 pos,
			Vec2i repeat)
		{
			return new Panel<V> (graph, flipVertically, true, repeat, texture)
				.Offset (new Vec3 (pos, 0f));
		}
	}
}
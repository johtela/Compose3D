namespace Compose3D.SceneGraph
{
	using System.Linq;
	using DataStructures;
	using Geometry;
	using GLTypes;
	using Maths;
	using OpenTK;
	using OpenTK.Input;
	using OpenTK.Graphics.OpenGL4;
	using Textures;

	public class Panel<V> : SceneNode 
		where V : struct, IVertex, ITextured
	{
		public Texture Texture { get; set; }

		private Geometry<V> _rectangle;
		private VBO<V> _vertexBuffer;
		private VBO<int> _indexBuffer;
		private bool _flipVertically;

		public Panel (SceneGraph graph, bool flipVertically)
			: base (graph)
		{
			_rectangle = Quadrilateral<V>.Rectangle (1f, 1f).Translate (0.5f, -0.5f);
			_flipVertically = flipVertically;
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

		public virtual bool Update (Vec2i viewportSize, MouseDevice mouse)
		{
			return false;
		}

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
	}
}
namespace Compose3D.SceneGraph
{
	using System.Drawing;
	using System.Linq;
	using OpenTK;
	using OpenTK.Input;
	using OpenTK.Graphics.OpenGL4;
	using Geometry;
	using Maths;
	using Reactive;
	using Textures;
	using Visuals;
	using UI;

	public class ControlPanel<V> : Panel<V>
		where V : struct, IVertex, ITextured
	{
		private Visual _visual;
		private Bitmap _bitmap;
		private Vec2i _viewPortSize;

		public Control Control { get; private set; }

		public ControlPanel (SceneGraph graph, Control control, Vec2i size, Reaction<Vec2> moved)
			: base (graph, true)
		{
			var panelMoved = moved == null ?
				null :
				moved.Select ((PointF pt) => 
					new Vec2 (pt.X * 2f / _viewPortSize.X, -pt.Y * 2f / _viewPortSize.Y));
			Control = new PanelFrame (control, "PanelFrame", panelMoved);
			_visual = control.ToVisual (new SizeF (size.X, size.Y));
			_bitmap = _visual.ToBitmap (new Size (size.X, size.Y),
				System.Drawing.Imaging.PixelFormat.Format32bppArgb, Control.Style);
			Texture = Texture.FromBitmap (_bitmap);
		}

		public void UpdateControl (Vec2i viewportSize, MouseDevice mouse)
		{
			_viewPortSize = viewportSize;
			var relPos = PanelCoordinatesAtMousePos (new Vec2i (mouse.X, mouse.Y), 
				viewportSize);
			if (relPos.X >= 0f && relPos.Y >= 0f)
				Control.HandleInput (new PointF (relPos.X, relPos.Y));
			var visual = Control.ToVisual (new SizeF (_bitmap.Width, _bitmap.Height));
			if (_visual != visual)
			{
				_visual = visual;
				_visual.UpdateBitmap (_bitmap, Control.Style);
				Texture.UpdateBitmap (_bitmap, TextureTarget.Texture2D, 0);
			}
		}

		public static void UpdateAll (SceneGraph sceneGraph, GameWindow window, Vec2i viewportSize)
		{
			Control._prevKeyboardState = Control._currKeyboardState;
			Control._currKeyboardState = window.Keyboard.GetState ();
			Control._prevMouseState = Control._currMouseState;
			Control._currMouseState = window.Mouse.GetState ();
			var panels = sceneGraph.Root.Traverse ().OfType<ControlPanel<V>> ();
			foreach (var panel in panels)
				panel.UpdateControl (viewportSize, window.Mouse);
		}

		public static SceneNode Movable (SceneGraph graph, Control control, Vec2i size, Vec2 pos)
		{
			TransformNode result = null;
			result = new ControlPanel<V> (graph, control, size, 
				React.By ((Vec2 offs) => result.Offset += new Vec3 (offs, 0f)))
				.Offset (new Vec3 (pos, 0f));
			return result;
		}
	}
}
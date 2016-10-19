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

		public Control Control { get; private set; }

		public ControlPanel (SceneGraph graph, Control control, Vec2i size)
			: base (graph, true)
		{
			Control = new PanelFrame (control, "PanelFrame");
			_visual = control.ToVisual (new SizeF (size.X, size.Y));
			_bitmap = _visual.ToBitmap (new Size (size.X, size.Y),
				System.Drawing.Imaging.PixelFormat.Format32bppArgb, Control.Style);
			Texture = Texture.FromBitmap (_bitmap);
		}

		public override bool Update (Vec2i viewportSize, MouseDevice mouse)
		{
			var result = base.Update (viewportSize, mouse);
			if (!result)
			{
				var relPos = PanelCoordinatesAtMousePos (new Vec2i (mouse.X, mouse.Y), 
					viewportSize);
				result = relPos.X >= 0f && relPos.Y >= 0f;
				if (result)
					Control.HandleInput (new PointF (relPos.X, relPos.Y));
			}
			var visual = Control.ToVisual (new SizeF (_bitmap.Width, _bitmap.Height));
			if (_visual != visual)
			{
				_visual = visual;
				_visual.UpdateBitmap (_bitmap, Control.Style);
				Texture.UpdateBitmap (_bitmap, TextureTarget.Texture2D, 0);
			}
			return result;
		}

		public static SceneNode Movable (SceneGraph graph, Control control, Vec2i size, Vec2 pos)
		{
			TransformNode result = null;
			result = new ControlPanel<V> (graph, control, size)
				.Offset (new Vec3 (pos, 0f));
			return result;
		}
	}
}
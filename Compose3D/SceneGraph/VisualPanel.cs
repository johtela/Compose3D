namespace Compose3D.SceneGraph
{
	using OpenTK.Graphics.OpenGL;
	using Geometry;
	using Maths;
	using Textures;
	using Visuals;
	using UI;
	using System.Drawing;
	using OpenTK.Input;

	public class ControlPanel<V> : Panel<V>
		where V : struct, IVertex, ITextured
	{
		private Visual _visual;
		private Bitmap _bitmap;

		public Control Control { get; private set; }

		public ControlPanel (SceneGraph graph, Control control, Vec2i size)
			: base (graph, true)
		{
			Control = control;
			_visual = control.ToVisual ();
			_bitmap = _visual.ToBitmap (new Size (size.X, size.Y),
				System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			Texture = Texture.FromBitmap (_bitmap);
		}

		public void UpdateControl (Vec2i viewportSize, MouseDevice mouse, 
			KeyboardDevice keyboard)
		{
			var relPos = PanelCoordinatesAtMousePos (new Vec2i (mouse.X, mouse.Y), 
				viewportSize);
			Control.HandleInput (mouse, keyboard, new PointF (relPos.X, relPos.Y));
			var visual = Control.ToVisual ();
			if (_visual != visual)
			{
				_visual = visual;
				_visual.UpdateBitmap (_bitmap);
				Texture.UpdateBitmap (_bitmap, TextureTarget.Texture2D, 0);
			}
		}
	}
}
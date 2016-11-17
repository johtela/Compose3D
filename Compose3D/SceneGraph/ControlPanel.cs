namespace Compose3D.SceneGraph
{
	using System.Drawing;
	using OpenTK.Input;
	using OpenTK.Graphics.OpenGL4;
	using Geometry;
	using Maths;
	using Textures;
	using Visuals;
	using UI;

	public class ControlPanel<V> : Panel<V>
		where V : struct, IVertex, ITextured
	{
		private Visual _visual;
		private Bitmap _bitmap;
		private Vec2i _size;

		public Control Control { get; private set; }

		public ControlPanel (SceneGraph graph, Control control, Vec2i size, bool movable)
			: base (graph, true, movable, new Vec2i (1))
		{
			Control = control;
			_size = size;
			_visual = control.ToVisual (new SizeF (size.X, size.Y));
			CreateBitmap ();
		}

		private void CreateBitmap ()
		{
			_bitmap = _visual.ToBitmap (new Size (_size.X, _size.Y),
				System.Drawing.Imaging.PixelFormat.Format32bppArgb, Control.Style);
			Texture = Texture.FromBitmap (_bitmap);
		}

		public override UpdateAction Update (Vec2i viewportSize, MouseDevice mouse)
		{
			var action = base.Update (viewportSize, mouse);
			if (action == UpdateAction.Done)
				return action;
			if (action == UpdateAction.HandleInput)
			{
				var relPos = PanelCoordinatesAtMousePos (new Vec2i (mouse.X, mouse.Y),
					viewportSize);
				if (relPos.X >= 0f && relPos.Y >= 0f)
					Control.HandleInput (new PointF (relPos.X, relPos.Y));
			}
			var visual = Control.ToVisual (new SizeF (_size.X, _size.Y));
			if (_visual != visual)
			{
				_visual = visual;
				if (_bitmap == null)
					CreateBitmap ();
				else
				{
					_visual.UpdateBitmap (_bitmap, Control.Style);
					Texture.UpdateBitmap (_bitmap, TextureTarget.Texture2D, 0);
				}
			}
			return UpdateAction.Done;
		}

		protected override void Resize (Vec2i size)
		{
			_size = size;
			_bitmap = null;
		}

		public static SceneNode Movable (SceneGraph graph, Control control, Vec2i size, Vec2 pos)
		{
			return new ControlPanel<V> (graph, control, size, true)
				.Offset (new Vec3 (pos, 0f));
		}
	}
}
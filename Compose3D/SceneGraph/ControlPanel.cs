﻿namespace Compose3D.SceneGraph
{
	using Geometry;
	using Maths;
	using Textures;
	using Visuals;
	using UI;
	using System.Drawing;
	using System.Linq;
	using OpenTK;
	using OpenTK.Input;
	using OpenTK.Graphics.OpenGL4;

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
			_visual = control.ToVisual (new SizeF (size.X, size.Y));
			_bitmap = _visual.ToBitmap (new Size (size.X, size.Y),
				System.Drawing.Imaging.PixelFormat.Format32bppArgb, Control.Style);
			Texture = Texture.FromBitmap (_bitmap);
		}

		public void UpdateControl (Vec2i viewportSize, MouseDevice mouse)
		{
			var relPos = PanelCoordinatesAtMousePos (new Vec2i (mouse.X, mouse.Y), 
				viewportSize);
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
	}
}
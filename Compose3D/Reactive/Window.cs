namespace Compose3D.Reactive
{
	using System;
	using OpenTK;
	using Arithmetics;

	public static class Window
	{
		public static void WhenRendered (this Reaction<double> reaction, GameWindow window)
		{
			window.RenderFrame += (sender, e) => reaction (e.Time);
		}

		public static void WhenResized (this Reaction<Vec2> reaction, GameWindow window)
		{
			window.Resize += (sender, e) => reaction (new Vec2 (window.ClientSize.Width, window.ClientSize.Height));
		}
	}
}


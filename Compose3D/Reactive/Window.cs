namespace Compose3D.Reactive
{
	using System;
	using OpenTK;
	using Arithmetics;

	public static class Window
	{
		public static void WhenRendered (this Reaction<double> reaction, GameWindow window)
		{
			window.RenderFrame += reaction.Map<FrameEventArgs, double> (e => e.Time).ToEvent ();
		}

		public static void WhenResized (this Reaction<Vec2> reaction, GameWindow window)
		{
			window.Resize += reaction.Map<EventArgs, Vec2> (e => 
				new Vec2 (window.ClientSize.Width, window.ClientSize.Height)).ToEvent ();
		}
	}
}


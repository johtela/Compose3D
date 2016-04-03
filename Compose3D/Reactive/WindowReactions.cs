namespace Compose3D.Reactive
{
	using System;
	using OpenTK;
	using Compose3D.Maths;

	public static class WindowReactions
	{
		public static void WhenRendered (this Reaction<FrameEventArgs> reaction, GameWindow window)
		{
			EventHandler<FrameEventArgs> handler = null;
			handler = (sender, args) =>
			{
				if (!reaction (args))
					window.RenderFrame -= handler;
			};
			window.RenderFrame += handler;
		}

		public static void WhenRendered (this Reaction<double> reaction, GameWindow window)
		{
			WhenRendered (reaction.Map<FrameEventArgs, double> (e => e.Time), window);
		}

		public static void WhenResized (this Reaction<EventArgs> reaction, GameWindow window)
		{
			EventHandler<EventArgs> handler = null;
			handler = (sender, args) =>
			{
				if (!reaction (args))
					window.Resize -= handler;
			};
			window.Resize += handler;
		}
		
		public static void WhenResized (this Reaction<Vec2> reaction, GameWindow window)
		{
			WhenResized (reaction.Map<EventArgs, Vec2> (e => 
				new Vec2 (window.ClientSize.Width, window.ClientSize.Height)), window);
		}
	}
}
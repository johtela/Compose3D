﻿namespace ComposeFX.Graphics.Reactive
{
	using System;
	using OpenTK;
	using Maths;
    using ComposeFX.Reactive;

	public static class WindowReactions
	{
		public static Reaction<Reaction<FrameEventArgs>> WhenRendered (this Reaction<FrameEventArgs> reaction, 
			GameWindow window)
		{
			return reaction.ToEvent (
				handler => window.RenderFrame += handler,
				handler => window.RenderFrame -= handler);
		}

		public static Reaction<Reaction<FrameEventArgs>> WhenRendered (this Reaction<double> reaction, 
			GameWindow window)
		{
			return WhenRendered (reaction.MapInput<FrameEventArgs, double> (e => e.Time), window);
		}

		public static Reaction<Reaction<EventArgs>> WhenResized (this Reaction<EventArgs> reaction, 
			GameWindow window)
		{
			return reaction.ToEvent (
				handler => window.Resize += handler,
				handler => window.Resize -= handler);
		}
		
		public static Reaction<Reaction<EventArgs>> WhenResized (this Reaction<Vec2> reaction, 
			GameWindow window)
		{
			return WhenResized (reaction.MapInput<EventArgs, Vec2> (e => 
				new Vec2 (window.ClientSize.Width, window.ClientSize.Height)), window);
		}
	}
}
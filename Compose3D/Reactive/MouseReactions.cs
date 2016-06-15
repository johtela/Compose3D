namespace Compose3D.Reactive
{
	using System;
	using OpenTK;
	using OpenTK.Input;
	using Compose3D.Maths;

	public static class MouseReactions
	{
		public static Reaction<Reaction<MouseMoveEventArgs>> WhenMouseMovesOn (
			this Reaction<MouseMoveEventArgs> reaction, GameWindow window)
		{
			return reaction.ToEvent (
				handler => window.Mouse.Move += handler,
				handler => window.Mouse.Move -= handler);
		}

		public static Reaction<Reaction<MouseMoveEventArgs>> WhenMouseXYChangesOn (this Reaction<Vec2> reaction, 
			GameWindow window)
		{
			return reaction.Select<MouseMoveEventArgs, Vec2> (e => new Vec2 (e.X, e.Y))
				.WhenMouseMovesOn (window);
		}

		public static Reaction<Reaction<MouseWheelEventArgs>> WhenMouseWheelRollsOn (
			this Reaction<MouseWheelEventArgs> reaction, GameWindow window)
		{
			return reaction.ToEvent (
				handler => window.Mouse.WheelChanged += handler,
				handler => window.Mouse.WheelChanged -= handler);
		}

		public static Reaction<Reaction<MouseWheelEventArgs>> WhenMouseWheelDeltaChangesOn (
			this Reaction<float> reaction, GameWindow window)
		{
			return reaction.Select<MouseWheelEventArgs, float> (e => e.DeltaPrecise)
				.WhenMouseWheelRollsOn (window);
		}
	}
}
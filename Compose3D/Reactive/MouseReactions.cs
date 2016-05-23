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
			return reaction.ToEvent<MouseMoveEventArgs> (
				handler => window.MouseMove += handler,
				handler => window.MouseMove -= handler);
		}

		public static Reaction<Reaction<MouseMoveEventArgs>> WhenMouseXYChangesOn (this Reaction<Vec2> reaction, 
			GameWindow window)
		{
			return reaction.Map<MouseMoveEventArgs, Vec2> (e => new Vec2 (e.X, e.Y))
				.WhenMouseMovesOn (window);
		}

		public static Reaction<Reaction<MouseWheelEventArgs>> WhenMouseWheelRollsOn (
			this Reaction<MouseWheelEventArgs> reaction, GameWindow window)
		{
			return reaction.ToEvent<MouseWheelEventArgs> (
				handler => window.MouseWheel += handler,
				handler => window.MouseWheel -= handler);
		}

		public static Reaction<Reaction<MouseWheelEventArgs>> WhenMouseWheelDeltaChangesOn (
			this Reaction<float> reaction, GameWindow window)
		{
			return reaction.Map<MouseWheelEventArgs, float> (e => e.DeltaPrecise)
				.WhenMouseWheelRollsOn (window);
		}
	}
}
namespace Compose3D.Reactive
{
	using System;
	using OpenTK;
	using OpenTK.Input;
	using Arithmetics;

	public static class Mouse
	{
		public static void WhenMouseMovesOn (this Reaction<MouseMoveEventArgs> reaction, GameWindow window)
		{
			window.MouseMove += (sender, e) => reaction (e);
		}

		public static void WhenMouseXYChangesOn (this Reaction<Vec2> reaction, GameWindow window)
		{
			reaction.Map<MouseMoveEventArgs, Vec2> (e => new Vec2 (e.X, e.Y))
				.WhenMouseMovesOn (window);
		}

		public static void WhenMouseWheelRollsOn (this Reaction<MouseWheelEventArgs> reaction, GameWindow window)
		{
			window.MouseWheel += (sender, e) => reaction (e);
		}

		public static void WhenMouseWheelDeltaChangesOn (this Reaction<float> reaction, GameWindow window)
		{
			reaction.Map<MouseWheelEventArgs, float> (e => e.DeltaPrecise)
				.WhenMouseWheelRollsOn (window);
		}
	}
}
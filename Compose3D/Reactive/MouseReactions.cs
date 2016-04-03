namespace Compose3D.Reactive
{
	using System;
	using OpenTK;
	using OpenTK.Input;
	using Compose3D.Maths;

	public static class MouseReactions
	{
		public static void WhenMouseMovesOn (this Reaction<MouseMoveEventArgs> reaction, GameWindow window)
		{
			EventHandler<MouseMoveEventArgs> handler = null;
			handler = (sender, args) =>
			{
				if (!reaction (args))
					window.MouseMove -= handler;
			};
			window.MouseMove += handler;
		}

		public static void WhenMouseXYChangesOn (this Reaction<Vec2> reaction, GameWindow window)
		{
			reaction.Map<MouseMoveEventArgs, Vec2> (e => new Vec2 (e.X, e.Y))
				.WhenMouseMovesOn (window);
		}

		public static void WhenMouseWheelRollsOn (this Reaction<MouseWheelEventArgs> reaction, GameWindow window)
		{
			EventHandler<MouseWheelEventArgs> handler = null;
			handler = (sender, args) =>
			{
				if (!reaction (args))
					window.MouseWheel -= handler;
			};
			window.MouseWheel += handler;
		}

		public static void WhenMouseWheelDeltaChangesOn (this Reaction<float> reaction, GameWindow window)
		{
			reaction.Map<MouseWheelEventArgs, float> (e => e.DeltaPrecise)
				.WhenMouseWheelRollsOn (window);
		}
	}
}
namespace Compose3D.Reactive
{
	using System;
	using OpenTK;
	using OpenTK.Input;
	using Arithmetics;

	public static class Mouse
	{
		public static void MouseMove (this Reaction<MouseMoveEventArgs> reaction, GameWindow window)
		{
			window.MouseMove += (object sender, MouseMoveEventArgs e) => reaction (e);
		}

		public static void MouseXY (this Reaction<Vec2> reaction, GameWindow window)
		{
			reaction.Map<MouseMoveEventArgs, Vec2> (e => new Vec2 (e.X, e.Y)).MouseMove (window);
		}
	}
}
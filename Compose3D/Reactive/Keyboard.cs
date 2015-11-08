namespace Compose3D.Reactive
{
	using System;
	using OpenTK;
	using OpenTK.Input;
	using Compose3D.Maths;

	public static class Keyboard
	{
		public static void WhenKeyDown (this Reaction<KeyboardKeyEventArgs> reaction, GameWindow window)
		{
			window.KeyDown += reaction.ToEvent ();
		}

		public static void WhenKeyDown (this Reaction<Key> reaction, GameWindow window)
		{
			reaction.Map<KeyboardKeyEventArgs, Key> (e => e.Key).WhenKeyDown (window);
		}

		public static void WhenKeyUp (this Reaction<KeyboardKeyEventArgs> reaction, GameWindow window)
		{
			window.KeyUp += reaction.ToEvent ();
		}

		public static void WhenKeyUp (this Reaction<Key> reaction, GameWindow window)
		{
			reaction.Map<KeyboardKeyEventArgs, Key> (e => e.Key).WhenKeyUp (window);
		}
	}
}


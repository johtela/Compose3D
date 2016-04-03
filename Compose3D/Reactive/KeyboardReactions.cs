namespace Compose3D.Reactive
{
	using System;
	using OpenTK;
	using OpenTK.Input;
	using Extensions;

	public static class KeyboardReactions
	{
		public static void WhenAnyKeyDown (this Reaction<KeyboardKeyEventArgs> reaction, GameWindow window)
		{
			EventHandler<KeyboardKeyEventArgs> handler = null;
			handler = (sender, args) =>
			{
				if (!reaction (args))
					window.KeyDown -= handler;
			};
			window.KeyDown += handler;
		}

		public static void WhenAnyKeyDown (this Reaction<Key> reaction, GameWindow window)
		{
			reaction.Map<KeyboardKeyEventArgs, Key> (e => e.Key).WhenAnyKeyDown (window);
		}

		public static void WhenKeyDown (this Reaction<Key> reaction, GameWindow window, params Key[] keys)
		{
			reaction.Filter (key => key.In (keys)).WhenAnyKeyDown (window);
		}

		public static void WhenAnyKeyUp (this Reaction<KeyboardKeyEventArgs> reaction, GameWindow window)
		{
			EventHandler<KeyboardKeyEventArgs> handler = null;
			handler = (sender, args) =>
			{
				if (!reaction (args))
					window.KeyUp -= handler;
			};
			window.KeyUp += handler;
		}

		public static void WhenAnyKeyUp (this Reaction<Key> reaction, GameWindow window)
		{
			reaction.Map<KeyboardKeyEventArgs, Key> (e => e.Key).WhenAnyKeyUp (window);
		}
	}
}


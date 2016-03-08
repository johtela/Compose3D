namespace Compose3D.Reactive
{
	using OpenTK;
	using OpenTK.Input;
	using Extensions;

	public static class Keyboard
	{
		public static void WhenAnyKeyDown (this Reaction<KeyboardKeyEventArgs> reaction, GameWindow window)
		{
			window.KeyDown += reaction.ToEvent ();
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
			window.KeyUp += reaction.ToEvent ();
		}

		public static void WhenAnyKeyUp (this Reaction<Key> reaction, GameWindow window)
		{
			reaction.Map<KeyboardKeyEventArgs, Key> (e => e.Key).WhenAnyKeyUp (window);
		}
	}
}


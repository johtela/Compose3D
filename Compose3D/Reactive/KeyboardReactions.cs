namespace Compose3D.Reactive
{
	using System;
	using OpenTK;
	using OpenTK.Input;
	using Extensions;

	public static class KeyboardReactions
	{
		public static Reaction<Reaction<KeyboardKeyEventArgs>> WhenAnyKeyDown (
			this Reaction<KeyboardKeyEventArgs> reaction, GameWindow window)
		{
			return reaction.ToEvent<KeyboardKeyEventArgs> (
				handler => window.KeyDown += handler,
				handler => window.KeyDown -= handler);
		}

		public static Reaction<Reaction<KeyboardKeyEventArgs>> WhenAnyKeyDown (this Reaction<Key> reaction, 
			GameWindow window)
		{
			return WhenAnyKeyDown (reaction.Select<KeyboardKeyEventArgs, Key> (e => e.Key), window);
		}

		public static Reaction<Reaction<KeyboardKeyEventArgs>> WhenKeyDown (this Reaction<Key> reaction, 
			GameWindow window, params Key[] keys)
		{
			return reaction.Where (key => key.In (keys)).WhenAnyKeyDown (window);
		}

		public static Reaction<Reaction<KeyboardKeyEventArgs>> WhenAnyKeyUp (
			this Reaction<KeyboardKeyEventArgs> reaction, GameWindow window)
		{
			return reaction.ToEvent<KeyboardKeyEventArgs> (
				handler => window.KeyUp += handler,
				handler => window.KeyUp -= handler);
		}

		public static Reaction<Reaction<KeyboardKeyEventArgs>> WhenAnyKeyUp (this Reaction<Key> reaction, 
			GameWindow window)
		{
			return WhenAnyKeyUp (reaction.Select<KeyboardKeyEventArgs, Key> (e => e.Key), window);
		}
	}
}


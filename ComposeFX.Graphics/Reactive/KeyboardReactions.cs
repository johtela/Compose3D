namespace ComposeFX.Graphics.Reactive
{
	using OpenTK;
	using OpenTK.Input;
	using Extensions;
    using ComposeFX.Reactive;

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
			return WhenAnyKeyDown (reaction.MapInput<KeyboardKeyEventArgs, Key> (e => e.Key), window);
		}

		public static Reaction<Reaction<KeyboardKeyEventArgs>> WhenKeyDown (this Reaction<Key> reaction, 
			GameWindow window, params Key[] keys)
		{
			return reaction.Filter (key => key.In (keys)).WhenAnyKeyDown (window);
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
			return WhenAnyKeyUp (reaction.MapInput<KeyboardKeyEventArgs, Key> (e => e.Key), window);
		}
	}
}


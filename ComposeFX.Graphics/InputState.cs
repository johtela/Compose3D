namespace ComposeFX.Graphics
{
	using System.Collections.Generic;
	using OpenTK;
	using OpenTK.Input;
	using Maths;
	using Extensions;

	public class InputState
	{
		public static InputState Current;
		public static InputState Previous;

		public readonly KeyboardState KeyboardState;
		public readonly MouseState MouseState;

		public static int RepeatDelay = 30;

		private static Dictionary<Key, int> _keyDownDuration = new Dictionary<Key, int> (); 

		public InputState (GameWindow window)
		{
			KeyboardState = window.Keyboard.GetState ();
			MouseState = window.Mouse.GetState ();
		}

		public static void Update (GameWindow window)
		{
			Previous = Current;
			Current = new InputState (window);
			if (Previous == null)
				Previous = Current;
		}

		public static bool MouseButtonDown (MouseButton button)
		{
			return Current.MouseState.IsButtonDown (button);
		}

		public static bool AnyMouseButtonDown ()
		{
			return Current.MouseState.IsAnyButtonDown;
		}

		public static bool MouseButtonPressed (MouseButton button)
		{
			return Previous.MouseState.IsButtonUp (button) && Current.MouseState.IsButtonDown (button);
		}

		public static int MouseWheelChange ()
		{
			return Current.MouseState.Wheel - Previous.MouseState.Wheel;
		}

		public static bool AnyMouseButtonPressed ()
		{
			return !Previous.MouseState.IsAnyButtonDown && Current.MouseState.IsAnyButtonDown;
		}

		public static Vec2 MouseDelta ()
		{
			return new Vec2 (Current.MouseState.X - Previous.MouseState.X,
				Current.MouseState.Y - Previous.MouseState.Y);
		}

		public static bool KeyDown (Key key)
		{
			return Current.KeyboardState.IsKeyDown (key);
		}

		public static bool AnyKeyDown ()
		{
			return Current.KeyboardState.IsAnyKeyDown;
		}

		public static bool KeyPressed (Key key, bool repeat)
		{
			if (Previous.KeyboardState.IsKeyUp (key) && Current.KeyboardState.IsKeyDown (key))
				return true;
			var duration = 0;
			if (repeat)
			{
				if (Current.KeyboardState.IsKeyDown (key))
					duration = _keyDownDuration [key] + 1;
				_keyDownDuration [key] = duration;
			}
			return duration > RepeatDelay;
		}

		public static bool AnyKeyPressed ()
		{
			return !Previous.KeyboardState.IsAnyKeyDown && Current.KeyboardState.IsAnyKeyDown;
		}

		public static Key? AnyOfTheKeysPressed (params Key[] keys)
		{
			foreach (var key in keys)
				if (KeyPressed (key, false))
					return key;
			return null;
		}

		private static Key[] _numberKeys =
		{
			Key.Number0, Key.Number1, Key.Number2, Key.Number3, Key.Number4, Key.Number5, Key.Number6, Key.Number7, Key.Number8, Key.Number9,
			Key.Keypad0, Key.Keypad1, Key.Keypad2, Key.Keypad3, Key.Keypad4, Key.Keypad5, Key.Keypad6, Key.Keypad7, Key.Keypad8, Key.Keypad9,
			Key.Period, Key.KeypadDecimal, Key.Comma, Key.Minus, Key.KeypadMinus
		};

		public static char AnyNumberKeyPressed ()
		{
			var key = AnyOfTheKeysPressed (_numberKeys);
			return (char)(
				key == null ? '\0' :
				key >= Key.Number0 && key <= Key.Number9 ? (int)key - (int)Key.Number0 + (int)'0':
				key >= Key.Keypad0 && key <= Key.Keypad9 ? (int)key - (int)Key.Keypad0 + (int)'0' :
				key.In (Key.Minus, Key.KeypadMinus) ? '-' : 
				'.');
		}
	}
}


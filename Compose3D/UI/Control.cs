namespace Compose3D.UI
{
	using System.Drawing;
	using System.Collections.Generic;
	using System.Linq;
	using OpenTK.Input;
	using Visuals;
	using Extensions;

	public abstract class Control : IVisualizable
	{
		internal static KeyboardState _currKeyboardState;
		internal static KeyboardState _prevKeyboardState;
		internal static MouseState _currMouseState;
		internal static MouseState _prevMouseState;

		public static VisualStyle SelectedStyle = 
			new VisualStyle (
				VisualStyle.Default,
				textBrush: Brushes.White,
				brush: Brushes.DarkGray);

		public static int RepeatDelay = 30;

		private static Dictionary<Key, int> _keyDownDuration = new Dictionary<Key, int> (); 

		public abstract Visual ToVisual ();

		public abstract void HandleInput (PointF relativeMousePos);

		public static bool MouseButtonDown (MouseButton button)
		{
			return _currMouseState.IsButtonDown (button);
		}

		public static bool AnyMouseButtonDown ()
		{
			return _currMouseState.IsAnyButtonDown;
		}

		public static bool MouseButtonPressed (MouseButton button)
		{
			return _prevMouseState.IsButtonUp (button) && _currMouseState.IsButtonDown (button);
		}

		public static bool AnyMouseButtonPressed ()
		{
			return !_prevMouseState.IsAnyButtonDown && _currMouseState.IsAnyButtonDown;
		}

		public static bool KeyDown (Key key)
		{
			return _currKeyboardState.IsKeyDown (key);
		}

		public static bool AnyKeyDown ()
		{
			return _currKeyboardState.IsAnyKeyDown;
		}

		public static bool KeyPressed (Key key, bool repeat)
		{
			if (_prevKeyboardState.IsKeyUp (key) && _currKeyboardState.IsKeyDown (key))
				return true;
			var duration = 0;
			if (repeat)
			{
				if (_currKeyboardState.IsKeyDown (key))
					duration = _keyDownDuration [key] + 1;
				_keyDownDuration [key] = duration;
			}
			return duration > RepeatDelay;
		}

		public static bool AnyKeyPressed ()
		{
			return !_prevKeyboardState.IsAnyKeyDown && _currKeyboardState.IsAnyKeyDown;
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
			Key.Period, Key.KeypadDecimal, Key.Comma
		};

		public static char AnyNumberKeyPressed ()
		{
			var key = AnyOfTheKeysPressed (_numberKeys);
			return (char)(
				key == null ? '\0' :
				key >= Key.Number0 && key <= Key.Number9 ? (int)key - (int)Key.Number0 + (int)'0':
				key >= Key.Keypad0 && key <= Key.Keypad9 ? (int)key - (int)Key.Keypad0 + (int)'0' :
				'.');
		}
	}
}

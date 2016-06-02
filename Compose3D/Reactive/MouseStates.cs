namespace Compose3D.Reactive
{
	using OpenTK.Input;

	public static class MouseStates
	{
		public static Reaction<MouseDevice> ButtonDown (MouseButton button)
		{
			return m => m[button];
		}

		public static Reaction<MouseDevice> ButtonUp (MouseButton button)
		{
			return m => !m[button];
		}

		public class ClickState
		{
			public readonly State<MouseDevice> Up;
			public readonly State<MouseDevice> Down;

			public ClickState (State<MouseDevice> up, State<MouseDevice> down)
			{
				Up = up;
				Down = down;
			}
		}

		public static ClickState ButtonClick (MouseButton button, Reaction<MouseDevice> pressed,
			Reaction<MouseDevice> clicked)
		{
			State<MouseDevice> mouseUp = null, mouseDown = null;
			mouseUp = ButtonDown (button).And (pressed).ToState (mouseDown);
			mouseDown = ButtonUp (button).And (clicked).ToState (mouseUp);
			return new ClickState (mouseUp, mouseDown);
		}
	}
}
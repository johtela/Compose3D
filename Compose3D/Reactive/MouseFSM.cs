namespace Compose3D.Reactive
{
	using OpenTK.Input;

	public static class MouseFSM
	{
		public static Reaction<MouseDevice> ButtonDown (MouseButton button)
		{
			return m => true; // m[button];
		}

		public static Reaction<MouseDevice> ButtonUp (MouseButton button)
		{
			return m => true; // !m[button];
		}

		public class Click : FSM<MouseDevice>
		{
			public readonly State<MouseDevice> Up;
			public readonly State<MouseDevice> Down;

			public Click (MouseButton button, Reaction<MouseDevice> pressed,
				Reaction<MouseDevice> clicked)
			{
				Up = ButtonDown (button).And (pressed).ToState (() => Down);
				Down = ButtonUp (button).And (clicked).ToState (() => Up);
				Current = Up;
			}
		} 
	}
}
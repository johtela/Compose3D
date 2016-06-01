namespace Compose3D.Reactive
{
	using OpenTK.Input;

	public static class MouseFSM
	{
		public static Reaction<MouseDevice> ButtonDown (MouseButton button)
		{
			return m => m[button];
		}

		public static Reaction<MouseDevice> ButtonUp (MouseButton button)
		{
			return m => !m[button];
		}

		public static FSM<MouseDevice> ButtonClick (MouseButton button, Reaction<MouseDevice> pressed, 
			Reaction<MouseDevice> clicked)
		{
			var mouseUp = new FSM<MouseDevice> ();
			var mouseDown = new FSM<MouseDevice> ();
			mouseUp.Add (ButtonDown (button).And (pressed), mouseDown);
			mouseDown.Add (ButtonUp (button).And (clicked), mouseUp);
			return mouseUp;
		}	
	}
}
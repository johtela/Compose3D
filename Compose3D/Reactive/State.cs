namespace Compose3D.Reactive
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	public delegate State<T> State<T> (T input);

	public static class State
	{
		public static State<T> Forever<T> (Action<T> action)
		{
			State<T> result = null;
			result = input =>
			{
				action (input);
				return result;
			};
			return result;
		}

		public static State<T> ToState<T> (this Reaction<T> reaction, State<T> nextState)
		{
			State<T> result = null;
			result = input => reaction (input) ? nextState : result;
			return result;
		}
	}
}

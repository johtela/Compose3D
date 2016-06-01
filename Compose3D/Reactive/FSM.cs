namespace Compose3D.Reactive
{
	using System;
	using System.Collections.Generic;

	public class FSM<T> : IEnumerable<Tuple<Reaction<T>, FSM<T>>>
	{
		private List<Tuple<Reaction<T>, FSM<T>>> _transitions;

		public FSM ()
		{
			_transitions = new List<Tuple<Reaction<T>, FSM<T>>> ();
		}

		public FSM<T> Transition (T input)
		{
			foreach (var transition in this)
				if (transition.Item1 (input))
					return transition.Item2;
			return this;
		}

		public void Add (Reaction<T> reaction, FSM<T> state)
		{
			_transitions.Add (Tuple.Create (reaction, state));
		}

		public IEnumerator<Tuple<Reaction<T>, FSM<T>>> GetEnumerator ()
		{
			return _transitions.GetEnumerator ();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}
	}
}
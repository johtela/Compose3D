namespace Compose3D.Reactive
{
	using System;

	public delegate void Reaction<T> (T input);

	public static class React
	{
		public static Reaction<T> By<T> (Action<T> action)
		{
			return new Reaction<T> (action);
		}

		public static Reaction<Tuple<T, U>> By<T, U> (Action<T, U> action)
		{
			return input => action (input.Item1, input.Item2);
		}

		public static Reaction<Tuple<T, U, V>> By<T, U, V> (Action<T, U, V> action)
		{
			return input => action (input.Item1, input.Item2, input.Item3);
		}

		public static Reaction<Tuple<T, U, V, W>> By<T, U, V, W> (Action<T, U, V, W> action)
		{
			return input => action (input.Item1, input.Item2, input.Item3, input.Item4);
		}

		public static Reaction<T> Map<T, U> (this Reaction<U> reaction, Func<T, U> func)
		{
			return input => reaction (func (input));
		}

		public static Reaction<T> Map<T> (this Reaction<T> reaction, Func<T, T> func)
		{
			return input => reaction (func (input));
		}

		public static Reaction<T> Filter<T> (this Reaction<T> reaction, Func<T, bool> predicate)
		{
			return input =>
			{
				if (predicate (input))
					reaction (input);
			};
		}

		public static Reaction<T> Join<T> (params Reaction<T>[] reactions)
		{
			return input =>
			{
				foreach (var r in reactions)
					r (input);
			};
		}

		public static Reaction<U> Fold<T, U> (this Reaction<T> reaction, Func<T, U, T> func, T initial)
		{
			var current = initial;
			return input =>
			{
				current = func (current, input);
				reaction (current);
			};
		}

		public static EventHandler<T> ToEvent<T> (this Reaction<T> reaction) where T : EventArgs
		{
			return (sender, e) => reaction (e);
		}
	}
}


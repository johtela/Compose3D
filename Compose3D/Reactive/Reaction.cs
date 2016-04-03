namespace Compose3D.Reactive
{
	using System;
	using System.Linq;
	using Extensions;

	public delegate bool Reaction<T> (T input);

	/// <summary>
	/// Operations for creating reactions and for extending and combining them in various ways.
	/// </summary>
	public static class React
	{
		public static Reaction<T> By<T> (Func<T, bool> func)
		{
			return new Reaction<T> (func);
		}

		public static Reaction<T> By<T> (Action<T> action)
		{
			return input =>
			{
				action (input);
				return true;
			};
		}

		public static Reaction<Tuple<T, U>> By<T, U> (Action<T, U> action)
		{
			return input =>
			{
				action (input.Item1, input.Item2);
				return true;
			};
		}

		public static Reaction<Tuple<T, U, V>> By<T, U, V> (Action<T, U, V> action)
		{
			return input =>
			{
				action (input.Item1, input.Item2, input.Item3);
				return true;
			};
		}

		public static Reaction<Tuple<T, U, V, W>> By<T, U, V, W> (Action<T, U, V, W> action)
		{
			return input =>
			{
				action (input.Item1, input.Item2, input.Item3, input.Item4);
				return true;
			};
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
			return input => predicate (input) ? reaction (input) : true;
		}

		public static Reaction<T> Reduce<T, U> (this Reaction<U> reaction, Func<U, T, U> func, U initial)
		{
			var current = initial;
			return input =>
			{
				current = func (current, input);
				return reaction (current);
			};
		}

		public static Reaction<T> Once<T> (this Reaction<T> reaction)
		{
			return input =>
			{
				reaction (input);
				return false;
			};
		}

		public static Reaction<T> Propagate<T> (params Reaction<T>[] reactions)
		{
			var done = new bool [reactions.Length];
			return input =>
			{
				var cont = false;
				for (int i = 0; i < reactions.Length; i++)
					if (!done[i])
					{
						done[i] = !reactions[i] (input);
						if (!done[i])
							cont = true;
					}
				return cont;
			};
		}

		public static Reaction<T> Buffer<T> (this Reaction<T[]> reaction, int bufferSize)
		{
			var buffer = new T[bufferSize];
			var last = 0;
			return input =>
			{
				buffer[last++] = input;
				if (last == bufferSize)
				{
					last = 0;
					return reaction (buffer);
				}
				return true;
			};
		}

		public static Reaction<T> Skip<T> (this Reaction<T> reaction, int count)
		{
			var i = 0;
			return input => ++i > count ? reaction (input) : true;
		}

		public static Reaction<T> SkipWhile<T> (this Reaction<T> reaction, Func<T, bool> condition)
		{
			return input => condition (input) ? true : reaction (input);
		}

		public static Reaction<T> Take<T> (this Reaction<T> reaction, int count)
		{
			var i = 0;
			return input => i++ < count ? reaction (input) : false;
		}

		public static Reaction<T> TakeWhile<T> (this Reaction<T> reaction, Func<T, bool> condition)
		{
			return input => condition (input) ? reaction (input) : false;
		}

		private class EventListener<T> where T : EventArgs
		{
			private Reaction<T> _reaction;
			private EventHandler<T> _eventHandler;
			
			public EventListener (Reaction<T> reaction, EventHandler<T> eventHandler)
			{
				_reaction = reaction;
				_eventHandler = eventHandler;
			}
			
			public void Listen (object sender, T eventArgs)
			{
				if (!_reaction (eventArgs))
					_eventHandler -= Listen;
			}
		}
		
		public static void ToEvent<T> (this Reaction<T> reaction, EventHandler<T> eventHandler) 
			where T : EventArgs
		{
			var listener = new EventListener<T> (reaction, eventHandler);
			eventHandler += listener.Listen;
		}
	}
}
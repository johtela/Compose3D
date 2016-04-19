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

		public static Reaction<T> Select<T, U> (this Reaction<U> reaction, Func<T, U> func)
		{
			return input => reaction (func (input));
		}

		public static Reaction<T> Select<T> (this Reaction<T> reaction, Func<T, T> func)
		{
			return input => reaction (func (input));
		}

		public static Reaction<T> Where<T> (this Reaction<T> reaction, Func<T, bool> predicate)
		{
			return input => predicate (input) ? reaction (input) : true;
		}

		public static Reaction<T> Aggregate<T, U> (this Reaction<U> reaction, Func<U, T, U> func, U initial)
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

		public static Reaction<T> And<T> (this Reaction<T> reaction, Reaction<T> other)
		{
			return input => reaction (input) ? other (input) : false;
		}

		public static Reaction<T> Or<T> (this Reaction<T> reaction, Reaction<T> other)
		{
			return input => reaction (input) ? true : other (input);
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

		public static Reaction<Reaction<T>> ToEvent<T> (this Reaction<T> reaction, 
			Action<EventHandler<T>> subscribe, Action<EventHandler<T>> unsubscribe)
			where T : EventArgs
		{
			return continuation =>
			{
				EventHandler<T> handler = null;
				handler = (sender, args) =>
				{
					if (!reaction (args))
					{
						unsubscribe (handler);
						continuation (args);
					}
				};
				subscribe (handler);
				return true;
			};
		}
		
		public static void Evoke<T> (this Reaction<Reaction<T>> reaction)
		{
			reaction (value => false);
		}

		public static bool Evoke<T, U> (this Reaction<Tuple<T, U>> reaction, T param1, U param2)
		{
			return reaction (Tuple.Create (param1, param2));
		}
	}
}
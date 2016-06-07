namespace Extensions
{
	using System;
	using System.Collections.Generic;

	public static class TupleExt
	{
		public static T First<T, U> (this Tuple<T, U> tuple)
		{
			return tuple.Item1;
		}

		public static U Second<T, U> (this Tuple<T, U> tuple)
		{
			return tuple.Item2;
		}

		public static void Bind<T, U> (this Tuple<T, U> tuple, Action<T, U> action)
		{
			action (tuple.Item1, tuple.Item2);
		}

		public static V Bind<T, U, V> (this Tuple<T, U> tuple, Func<T, U, V> func)
		{
			return func (tuple.Item1, tuple.Item2);
		}

		public static bool Match<T, U> (this Tuple<T, U> tuple, T first, out U second)
		{
			if (tuple.Item1.Equals (first))
			{
				second = tuple.Item2;
				return true;
			}
			else
			{
				second = default (U);
				return false;
			}
		}

		public static bool Match<T, U> (this Tuple<T, U> tuple, out T first, U second)
		{
			if (tuple.Item2.Equals (second))
			{
				first = tuple.Item1;
				return true;
			}
			else
			{
				first = default (T);
				return false;
			}
		}

		public static bool Match<T, U> (this Tuple<T, U> tuple, Func<T, bool> predicate, out U second)
		{
			if (predicate (tuple.Item1))
			{
				second = tuple.Item2;
				return true;
			}
			else
			{
				second = default (U);
				return false;
			}
		}

		public static Tuple<T, U> EmptyTuple<T, U> ()
		{
			return Tuple.Create (default (T), default (U));
		}

		public static Tuple<T, U, V> EmptyTuple<T, U, V> ()
		{
			return Tuple.Create (default (T), default (U), default (V));
		}

		public static Tuple<T, U, V, W> EmptyTuple<T, U, V, W> ()
		{
			return Tuple.Create (default (T), default (U), default (V), default (W));
		}

		public static bool HasValues<T, U> (this Tuple<T, U> tuple)
		{
			return tuple.Item1.NotDefault () && tuple.Item2.NotDefault ();
		}

		public static bool HasValues<T, U, V> (this Tuple<T, U, V> tuple)
		{
			return tuple.Item1.NotDefault () && tuple.Item2.NotDefault () && tuple.Item3.NotDefault ();
		}

		public static bool HasValues<T, U, V, W> (this Tuple<T, U, V, W> tuple)
		{
			return tuple.Item1.NotDefault () && tuple.Item2.NotDefault () && tuple.Item3.NotDefault () &&
				tuple.Item4.NotDefault ();
		}

		public static void Switch<T, U> (this Tuple<T, U> tuple, Action<T> first, Action<U> second)
		{
			if (tuple.Item1.NotDefault ())
				first (tuple.Item1);
			else if (tuple.Item2.NotDefault ())
				second (tuple.Item2);
		}

		public static void Switch<T, U, V> (this Tuple<T, U, V> tuple, Action<T> first, Action<U> second,
			Action<V> third)
		{
			if (tuple.Item1.NotDefault ())
				first (tuple.Item1);
			else if (tuple.Item2.NotDefault ())
				second (tuple.Item2);
			else if (tuple.Item3.NotDefault ())
				third (tuple.Item3);
		}

		public static void Switch<T, U, V, W> (this Tuple<T, U, V, W> tuple, Action<T> first, Action<U> second,
			Action<V> third, Action<W> fourth)
		{
			if (tuple.Item1.NotDefault ())
				first (tuple.Item1);
			else if (tuple.Item2.NotDefault ())
				second (tuple.Item2);
			else if (tuple.Item3.NotDefault ())
				third (tuple.Item3);
			else if (tuple.Item4.NotDefault ())
				fourth (tuple.Item4);
		}

		public static void ForAll<T, U> (this Tuple<T, U> tuple, Action<T> first, Action<U> second)
		{
			first (tuple.Item1);
			second (tuple.Item2);
		}

		public static void ForAll<T, U, V> (this Tuple<T, U, V> tuple, Action<T> first, Action<U> second,
			Action<V> third)
		{
			first (tuple.Item1);
			second (tuple.Item2);
			third (tuple.Item3);
		}

		public static void ForAll<T, U, V, W> (this Tuple<T, U, V, W> tuple, Action<T> first, Action<U> second,
			Action<V> third, Action<W> fourth)
		{
			first (tuple.Item1);
			second (tuple.Item2);
			third (tuple.Item3);
			fourth (tuple.Item4);
		}

		public static Tuple<U, T> Swap<T, U> (this Tuple<T, U> tuple)
		{
			return Tuple.Create (tuple.Item2, tuple.Item1);
		}

		public static void Add<T, U> (this IList<Tuple<T, U>> list,
			T item1, U item2)
		{
			list.Add (Tuple.Create (item1, item2));
		}

		public static void Add<T, U, V> (this IList<Tuple<T, U, V>> list,
				T item1, U item2, V item3)
		{
			list.Add (Tuple.Create (item1, item2, item3));
		}
	}
}

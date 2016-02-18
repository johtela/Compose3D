namespace Extensions
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	public static class TupleExt
	{
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
	}
}

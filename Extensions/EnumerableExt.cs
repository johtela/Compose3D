namespace Extensions
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	public static class EnumerableExt
	{
		public static IEnumerable<T> AsEnumerable<T> (this T value)
		{
			yield return value;
		}

		public static IEnumerable<T> AsPrintable<T> (this IEnumerable<T> enumerable)
		{
			return new PrintableEnumerable<T> (enumerable);
		}

		public static bool IsEmpty<T> (this IEnumerable<T> enumerable)
		{
			return !enumerable.GetEnumerator ().MoveNext ();
		}

		public static IEnumerable<T> Append<T> (this IEnumerable<T> enumerable, T item)
		{
			foreach (T i in enumerable)
				yield return i;
			yield return item;
		}

		public static IEnumerable<T> Prepend<T> (this IEnumerable<T> enumerable, T item)
		{
			yield return item;
			foreach (T i in enumerable)
				yield return i;
		}

		public static IEnumerable<T> Loop<T> (this IEnumerable<T> enumerable)
		{
			while (true)
				foreach (T i in enumerable)
					yield return i;
		}

		public static IEnumerable<U> Collect<T, U> (this IEnumerable<T> enumerable, Func<T, IEnumerable<U>> func)
		{
			foreach (var outer in enumerable)
				foreach (var inner in func (outer))
					yield return inner;
		}

		public static IEnumerable<T> Collect<T> (this IEnumerable<IEnumerable<T>> enumerable)
		{
			return Collect (enumerable, Fun.Identity);
		}

		public static IEnumerable<V> Combine<T, U, V> (this IEnumerable<T> enum1,
			IEnumerable<U> enum2, Func<T, U, V> combine)
		{
			var e1 = enum1.GetEnumerator ();
			var e2 = enum2.GetEnumerator ();
			var i1 = default (T);
			var i2 = default (U);

			while (true)
			{
				var b1 = e1.MoveNext ();
				var b2 = e2.MoveNext ();

				if (!b1 && !b2) break;
				if (b1) i1 = e1.Current;
				if (b2) i2 = e2.Current;
				yield return combine (i1, i2);
			}
		}

		public static void ForEach<T> (this IEnumerable<T> enumerable, Action<T> action)
		{
			foreach (var x in enumerable)
				action (x);
		}

		public static T[,] To2DArray<T> (this IEnumerable<T> enumerable, int dimension1, int dimension2)
		{
			var res = new T[dimension1, dimension2];
			var e = enumerable.GetEnumerator ();
			for (int i = 0; i < dimension1; i++)
				for (int j = 0; j < dimension2; j++)
				{
					res[i, j] = e.Current;
					e.MoveNext ();
				}
			return res;
		}
	}
}

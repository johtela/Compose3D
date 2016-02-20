﻿namespace Extensions
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Extensions;

	public static class EnumerableExt
	{
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
			return enumerable.Concat (Enumerate (item));
		}

		public static IEnumerable<T> Prepend<T> (this IEnumerable<T> enumerable, T item)
		{
			return Enumerate (item).Concat (enumerable);
		}

		public static void ForEach<T> (this IEnumerable<T> enumerable, Action<T> action)
		{
			foreach (var x in enumerable)
				action (x);
		}

		public static void ForEach<T, U> (this IEnumerable<Tuple<T, U>> items, Action<T, U> action)
		{
			foreach (var item in items)
				action (item.Item1, item.Item2);
		}

		public static void ForEach<T, U, V> (this IEnumerable<Tuple<T, U, V>> items, Action<T, U, V> action)
		{
			foreach (var item in items)
				action (item.Item1, item.Item2, item.Item3);
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

		public static T Next<T> (this IEnumerator<T> enumerator)
		{
			enumerator.MoveNext ();
			return enumerator.Current;
		}

		public static T Next<T> (ref IEnumerable<T> enumerable)
		{
			var result = enumerable.First ();
			enumerable = enumerable.Skip (1);
			return result;
		}

		public static IEnumerable<T> Repeat<T> (this IEnumerable<T> enumerable)
		{
			while (true)
			{
				var enumerator = enumerable.GetEnumerator ();
				while (enumerator.MoveNext ())
					yield return enumerator.Current;
			}
		}

		private static IEnumerable<T[]> EnumerateCombinations<T> (this IEnumerable<T>[] values, T[] result,
			int index)
		{
			foreach (var x in values[index])
			{
				result[index] = x;
				if (index == values.Length - 1)
					yield return result;
				else
					foreach (var v in values.EnumerateCombinations (result, index + 1))
						yield return v;
			}
		}

		public static IEnumerable<T[]> Combinations<T> (this T[] vector, Func<T, IEnumerable<T>> project)
		{
			return EnumerateCombinations (vector.Map (project), new T[vector.Length], 0);
		}

		public static string SeparateWith<T> (this IEnumerable<T> lines, string separator)
		{
			return lines.Any () ? lines.Select (l => l.ToString ()).Aggregate ((s1, s2) => s1 + separator + s2) : "";
		}

		public static IEnumerable<T> MinimumItems<T> (this IEnumerable<T> items, Func<T, float> selector)
		{
			var res = new List<T> ();
			var min = float.MaxValue;
			foreach (var item in items)
			{
				var value = selector (item);
				if (value < min)
				{
					min = value;
					res.Clear ();
					res.Add (item);
				}
				else if (value == min)
					res.Add (item);
			}
			return res;
		}

		public static IEnumerable<T> MaximumItems<T> (this IEnumerable<T> items, Func<T, float> selector)
		{
			var res = new List<T> ();
			var max = float.MinValue;
			foreach (var item in items)
			{
				var value = selector (item);
				if (value.ApproxEquals (max))
					res.Add (item);
				else if (value > max)
				{
					max = value;
					res.Clear ();
					res.Add (item);
				}
			}
			return res;
		}

		public static IEnumerable<float> Range (float start, float end, float step)
		{
			for (float val = start; step > 0 ? val <= end : val >= end; val += step)
				yield return val;
		}

		public static IEnumerable<T> RemoveConsequtiveDuplicates<T> (this IEnumerable<T> items)
		{
			var prev = default (T);
			var first = true;
			foreach (var item in items)
			{
				if (first || !item.Equals (prev))
				{
					yield return item;
					first = false;
				}
				prev = item;
			}
		}

		public static IEnumerable<T> Enumerate<T> (params T[] items)
		{
			return items;
		}

		public static IEnumerable<K> Keys<K, V> (this IEnumerable<KeyValuePair<K, V>> pairs)
		{
			return pairs.Select (kv => kv.Key);
		}

		public static IEnumerable<V> Values<K, V> (this IEnumerable<KeyValuePair<K, V>> pairs)
		{
			return pairs.Select (kv => kv.Value);
		}

		public static IEnumerable<T> WhenOfType<T, U> (this IEnumerable<T> items, Action<U> action)
			where U : T
		{
			foreach (var node in items)
				if (node is U)
					action ((U)node);
				else
					yield return node;
		}

		public static void ToVoid<T> (this IEnumerable<T> items)
		{
			foreach (var item in items) ;
		}
	}
}
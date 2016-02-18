namespace Extensions
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	public static class ArrayExt
	{
		public static T[] Segment<T> (this T[] array, int first, int length)
		{
			if (first < 0 || first >= array.Length)
				throw new ArgumentException ("First is out of array index range", "first");
			if (length < 0 || (first + length) > array.Length)
				throw new ArgumentException ("Length is out of array index range", "length");
			var result = new T[length];
			Array.Copy (array, first, result, 0, length);
			return result;
		}

		public static U ReduceLeft<T, U> (this T[] array, U acc, Func<U, T, U> func)
		{
			for (int i = 0; i < array.Length; i++)
				acc = func (acc, array[i]);
			return acc;
		}

		public static U ReduceRight<T, U> (this T[] array, Func<T, U, U> func, U acc)
		{
			for (int i = array.Length - 1; i >= 0; i--)
				acc = func (array[i], acc);
			return acc;
		}

		public static string ToString<T> (this T[] array, string openBracket, string closeBracket, string separator)
		{
			StringBuilder sb = new StringBuilder (openBracket);

			for (int i = 0; i < array.Length; i++)
			{
				sb.Append (array[i]);

				if (i < (array.Length - 1))
					sb.Append (separator);
			}
			sb.Append (closeBracket);
			return sb.ToString ();
		}

		public static void Swap<T> (this T[] array, int i, int j)
		{
			var t = array[i];
			array[i] = array[j];
			array[j] = t;
		}

		public static string AsString<T> (this T[] array)
		{
			var sb = new StringBuilder ("{ ");
			var len = array.Length;
			if (len > 0)
			{
				for (int i = 0; i < len - 1; i++)
					sb.AppendFormat ("{0}, ", array[i].ToString ());
				sb.AppendFormat ("{0} ", array[len - 1].ToString ());
			}
			sb.Append ("}");
			return sb.ToString ();
		}

		/// <summary>
		/// Heap's algorithm to generate permutations.
		/// </summary>
		private static IEnumerable<T[]> GeneratePermutations<T> (int n, T[] array)
		{
			if (n <= 0)
				yield return array;
			else
			{
				for (int i = 0; i <= n; i++)
				{
					foreach (var a in GeneratePermutations (n - 1, array))
						yield return a;
					var j = n % 2 == 1 ? 0 : i;
					array.Swap (j, n);
				}
			}
		}

		public static IEnumerable<T[]> Permutations<T> (this T[] array)
		{
			var res = new T[array.Length];
			array.CopyTo (res, 0);
			return GeneratePermutations (array.Length - 1, res);
		}

	}
}

namespace Compose3D
{
	using System;
	using System.Collections.Generic;
    using System.Linq;

    public static class Ext
	{
		public static bool In<T> (this T obj, params T[] alternatives)
		{
			return alternatives.Contains (obj);
		}

		public static bool IsDefault<T> (this T obj)
		{
			return obj.Equals (default (T));
		}

		public static bool NotDefault<T> (this T obj)
		{
			return !obj.Equals (default (T));
		}

        public static S Match<T, S> (this object expr, Func<T, S> func) 
            where T : class
            where S : class
        {
            var casted = expr as T;
            return casted != null ? func (casted) : null;
        }

        #region float extensions

        public static bool ApproxEquals (this float x, float y, float epsilon)
        {
            if (x == y)
                return true;

            float absX = Math.Abs (x);
            float absY = Math.Abs (y);
            float diff = Math.Abs (x - y);

            if (x * y == 0)
                return diff < (epsilon * epsilon);
            else
                return diff / (absX + absY) < epsilon;
        }

        public static bool ApproxEquals (this float x, float y)
        {
            return x.ApproxEquals (y, 0.000001f);
        }

		public static bool ApproxEquals<T> (this T x, T y)
			where T : struct, IEquatable<T>
		{
			if (typeof(T) == typeof (float))
				return ApproxEquals ((float)((object)x), (float)((object)y));
			else
				throw new ArgumentException ("This method is only defined for floats.");
		}

        #endregion

        #region Extensions for 1-dimensional arrays

        public static T[] Repeat<T> (this T value, int times)
        {
            var result = new T[times];
            for (int i = 0; i < times; i++)
                result[i] = value;
            return result;
        }

		public static T First<T>(this T[] array)
		{
			return array[0];
		}

		public static T Last<T> (this T[] array)
		{
			return array[array.Length - 1];
		}

        #endregion

        #region Extensions for 2-dimensional arrays

        public static T[][] Duplicate<T> (this T[][] matrix)
        {
            var res = new T[matrix.Length][];
            for (int i = 0; i < matrix.Length; i++)
            {
                var len = matrix[i].Length;
                res[i] = new T[len];
                Array.Copy (matrix[i], res[i], len);
            }
            return res;
        }

        public static int Columns<T> (this T[,] matrix)
        {
            return matrix.GetLength (0);
        }

        public static int Rows<T> (this T[,] matrix)
        {
            return matrix.GetLength (1);
        }

        public static T[] GetColumn<T> (this T[,] matrix, int col)
        {
            var rows = matrix.Rows ();
            var result = new T[rows];
            for (int r = 0; r < rows; r++)
                result[r] = matrix[col, r];
            return result;
        }

        public static void SetColumn<T> (this T[,] matrix, int col, T[] vector)
        {
            for (int r = 0; r < matrix.Rows (); r++)
                matrix[col, r] = vector[r];
        }

        public static T[] GetRow<T> (this T[,] matrix, int row)
        {
            var cols = matrix.Columns ();
            var result = new T[cols];
            for (int c = 0; c < cols; c++)
                result[c] = matrix[c, row];
            return result;
        }

        public static void SetRow<T> (this T[,] matrix, int row, T[] vector)
        {
            for (int c = 0; c < matrix.Columns (); c++)
                matrix[c, row] = vector[c];
        }

        #endregion
        
        #region IEnumerable extensions
		        
        public static T Next<T> (this IEnumerator<T> enumerator)
		{
			enumerator.MoveNext ();
			return enumerator.Current;
		}

        public static IEnumerable<T> Prepend<T> (this IEnumerable<T> enumerable, T item)
        {
            yield return item;
            foreach (T i in enumerable)
                yield return i;
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
			var res =  new List<T> ();
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
				if (value > max)
				{
					max = value;
					res.Clear ();
					res.Add (item);
				}
				else if (value == max)
					res.Add (item);
			}
			return res;
		}

		public static IEnumerable<float> Range (float start, float end, float step)
		{
			for (float val = start; step > 0 ? val <= end : val >= end; val += step)
				yield return val;
		}

        #endregion        
        
        #region Map & Fold
        
        public static U[] Map<T, U> (this T[] vector, Func<T, U> func)
        {
            var result = new U[vector.Length];
            Map (vector, result, func);
            return result;
        }

        public static void Map<T, U> (this T[] vector, U[] result, Func<T, U> func)
        {
            for (int i = 0; i < vector.Length; i++)
                result[i] = func (vector[i]);
        }

        public static void Map<T, U> (this T[,] matrix, U[,] result, Func<T, U> func)
        {
            for (int c = 0; c < matrix.Columns (); c++)
                for (int r = 0; r < matrix.Rows (); r++)
                    result[c, r] = func (matrix[c, r]);
        }

        public static V[] MapWith<T, U, V> (this T[] vector, U[] other, Func<T, U, V> func)
        {
            var result = new V[vector.Length];
            MapWith (vector, other, result, func);
            return result;
        }

        public static void MapWith<T, U, V> (this T[] vector, U[] other, V[] result, Func<T, U, V> func)
        {
            for (int i = 0; i < vector.Length; i++)
                result[i] = func (vector[i], other[i]);
        }

        public static void MapWith<T, U, V> (this T[,] matrix, U[,] other, V[,] result, Func<T, U, V> func)
        {
            for (int c = 0; c < matrix.Columns (); c++)
                for (int r = 0; r < matrix.Rows (); r++)
                    result[c, r] = func (matrix[c, r], other[c, r]);
        }

        public static U Fold<T, U> (this T[] vector, Func<U, T, U> func, U value)
        {
            for (int i = 0; i < vector.Length; i++)
                value = func (value, vector[i]);
            return value;
        }

        public static U Fold<T, U> (this T[,] matrix, Func<U, T, U> func, U value)
        {
            for (int c = 0; c < matrix.Columns (); c++)
                for (int r = 0; r < matrix.Rows (); r++)
                    value = func (value, matrix[c, r]);
            return value;
        }

        public static T Fold1<T> (this T[] vector, Func<T, T, T> func)
        {
            if (vector.Length < 1) throw new ArgumentException ("Vector cannot be empty", "vector");
            var value = vector[0];
            for (int i = 1; i < vector.Length; i++)
                value = func (value, vector[i]);
            return value;
        }

        public static V FoldWith<T, U, V> (this T[] vector, U[] other, Func<V, T, U, V> func, V value)
        {
            for (int i = 0; i < vector.Length; i++)
                value = func (value, vector[i], other[i]);
            return value;
        }

        #endregion

        #region Matrix operations
        
        public static void Transpose<T> (this T[,] matrix, T[,] result)
        {
            if (matrix.Rows () != result.Columns () || matrix.Columns () != result.Rows ())
                throw new ArgumentException (
                    string.Format ("{0}x{1} matrix cannot be transposed to {2}x{3} matrix",
                        matrix.Columns (), matrix.Rows (), result.Columns (), result.Rows ()));
            for (int c = 0; c < matrix.Columns (); c++)
                for (int r = 0; r < matrix.Rows (); r++)
                    result[r, c] = matrix[c, r];
        }

        public static void Multiply<T> (this T[,] matrix, T[,] other, T[,] result, Func<T, T, T, T> func)
        {
            if (matrix.Columns () != other.Rows ())
                throw new ArgumentException (
                    string.Format ("{0}x{1} matrix cannot be multiplied with {2}x{3} matrix",
                        matrix.Columns (), matrix.Rows (), other.Columns (), other.Rows ()));
            if (matrix.Rows () != result.Columns () || other.Columns () != result.Rows ())
                throw new ArgumentException (
                    string.Format ("Result matrix dimensions must be {0}x{1}", matrix.Rows (), other.Columns ()));
            for (int c = 0; c < result.Columns (); c++)
                for (int r = 0; r < result.Rows (); r++)
                {
                    var sum = default (T);
                    for (int k = 0; k < matrix.Columns (); k++)
                        sum = func (sum, matrix[k, r], other[c, k]);
                    result[c, r] = sum;
                }
        }

        public static void Multiply<T> (this T[,] matrix, T[] vector, T[] result, Func<T, T, T, T> func)
        {
            var cols = matrix.Columns ();
            var rows = matrix.Rows ();
            if (cols != vector.Length)
                throw new ArgumentException (
                    string.Format ("{0}x{1} matrix cannot be multiplied with {2}-component vector",
                        cols, rows, vector.Length));
            if (rows != result.Length)
                throw new ArgumentException (string.Format ("Result vector length must be {0}", rows));
            for (int r = 0; r < rows; r++)
            {
                var sum = default (T);
                for (int c = 0; c < cols; c++)
                    sum = func (sum, matrix[c, r], vector[c]);
                result[r] = sum;
            }
        }

        #endregion

		#region Tuple extensions

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

		#endregion
	}
}
namespace Compose3D
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
    using OpenTK;

	public static class Ext
	{
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

        public static T Next<T> (this IEnumerator<T> enumerator)
		{
			enumerator.MoveNext ();
			return enumerator.Current;
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

        public static Vector3 Transform (this Vector3 vec, Matrix3 mat)
        {
            return new Vector3 (
                vec.X * mat.Row0.X + vec.Y * mat.Row1.X + vec.Z * mat.Row2.X, 
                vec.X * mat.Row0.Y + vec.Y * mat.Row1.Y + vec.Z * mat.Row2.Y, 
                vec.X * mat.Row0.Z + vec.Y * mat.Row1.Z + vec.Z * mat.Row2.Z);
        }
	}
}
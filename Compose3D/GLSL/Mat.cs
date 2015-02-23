using System;
using System.Text;

namespace Compose3D.GLSL
{
    public class Mat<T> : IEquatable<Mat<T>> where T : struct, IEquatable<T>
    {
        private T[,] _matrix;

        protected Mat (T[,] matrix)
        {
            _matrix = matrix;
        }

        public T[,] Matrix
        {
            get { return _matrix; }
        }

        public int Columns
        {
            get { return _matrix.GetLength (0); }
        }

        public int Rows
        {
            get { return _matrix.GetLength (1); }
        }

        public T this[int col, int row]
        {
            get { return _matrix[col, row]; }
            set { _matrix[col, row] = value; }
        }

        public bool Equals (Mat<T> other)
        {
            for (int c = 0; c < Columns; c++)
                for (int r = 0; r < Rows; r++)
                    if (!_matrix[c, r].Equals (other._matrix[c, r])) return false;
            return true;
        }

        public override bool Equals (object obj)
        {
            var other = obj as Mat<T>;
            return !ReferenceEquals (other, null) && _matrix.Equals (other._matrix);
        }

        public override int GetHashCode ()
        {
            return _matrix.GetHashCode ();
        }

        public override string ToString ()
        {
            var sb = new StringBuilder ();
            sb.AppendLine ();
            for (int r = 0; r < Rows; r++)
            {
                sb.Append ("[");
                for (int c = 0; c < Columns; c++)
                    sb.AppendFormat (" {0}", _matrix[c, r]);
                sb.AppendLine (" ]");
            }
            return sb.ToString ();
        }

        public T[] ToArray ()
        {
            var res = new T[Rows * Columns];
            for (int i = 0, c = 0; c < Columns; c++)
                for (int r = 0; r < Rows; r++)
                    res[i++] = _matrix[c, r];
            return res;
        }

        public M ConvertTo<M> () where M : Mat<T>, new()
        {
            var res = new M ();
            for (int c = 0; c < Math.Min (Columns, res.Columns); c++)
                for (int r = 0; r < Math.Min (Rows, res.Rows); r++)
                    res[c, r] = this[c, r];
            return res;
        }

        public static bool operator == (Mat<T> left, Mat<T> right)
        {
            return left.Equals (right);
        }

        public static bool operator != (Mat<T> left, Mat<T> right)
        {
            return !left.Equals (right);
        }

        public static M Create<M> (T[,] values) where M : Mat<T>, new ()
        {
            var res = new M ();
            res._matrix = values;
            return res;
        }

        public static M Create<M> (params T[] values) where M : Mat<T>, new ()
        {
            var res = new M ();
            var len = values.Length;
            for (int i = 0, c = 0; c < res.Columns; c++)
                for (int r = 0; r < res.Rows; r++)
                {
                    res._matrix[c, r] = values[i];
                    i = (i + 1) % len;
                }
            return res;
        }
    }

    public static class Mat
    {
        public static void Transpose<M, R, T> (this M mat, R res) 
            where M : Mat<T>
            where R : Mat<T>
            where T : struct, IEquatable<T>
        {
            if (mat.Rows != res.Columns || mat.Columns != res.Rows)
                throw new ArgumentException (
                    string.Format ("{0}x{1} matrix cannot be transposed to {2}x{3} matrix",
                        mat.Columns, mat.Rows, res.Columns, res.Rows));
            for (int c = 0; c < mat.Columns; c++)
                for (int r = 0; r < mat.Rows; r++)
                    res[r, c] = mat[c, r];
        }

        public static M Transpose<M, T> (this M mat)
            where M : Mat<T>, new()
            where T : struct, IEquatable<T>
        {
            var res = new M ();
            Transpose<M, M, T> (mat, res);
            return res;
        }

        public static M Map<M, T> (this M mat, Func<T, T> func)
            where M : Mat<T>, new()
            where T : struct, IEquatable<T>
        {
            var res = new M ();
            mat.Matrix.Map (res.Matrix, func);
            return res;
        }

        public static M MapWith<M, T> (this M mat, M other, Func<T, T, T> func)
            where M : Mat<T>, new ()
            where T : struct, IEquatable<T>
        {
            var res = new M ();
            mat.Matrix.MapWith (other.Matrix, res.Matrix, func);
            return res;
        }

        public static M Multiply<M, T> (this M mat, M other, Func<T, T, T, T> func)
            where M : Mat<T>, new()
            where T : struct, IEquatable<T>
        {
            var res = new M ();
            mat.Matrix.Multiply (other.Matrix, res.Matrix, func);
            return res;
        }
    }
}
namespace ComposeFX.Maths
{
    using System;
	using Extensions;

	/// <summary>
	/// Interface for non-square matrices. 
	/// </summary>
	/// ComposeFX defines matrices in a column-major manner same as GLSL. This means that
	/// a \f$ n \times m\f$ matrix consist of n columns and m rows. If \f$ n \neq m \f$ then the 
	/// matrix is non-square.
	/// 
	/// *Note!* OpenTK defines matrices in a row-major way. So, if you use the OpenTK's
	/// math module with ComposeFX take that into account when converting matrices back and 
	/// forth. ComposeFX does not contain any conversion methods to OpenTK matrix types, and
	/// discourages their use in general.
	/// 
	/// Non-square matrices do not support all the operations that square matrices do. 
	/// Specifically they implement only addition subtraction, vector multiplication and 
	/// scalar multiplication and division. 
	/// 
	/// The purpose of this interface is mostly to enable testing the matrix implementations
	/// generically. It is not used elsewhere currently.
	/// <typeparam name="M">The type of the matrix struct implementing this interface.</typeparam>
	/// <typeparam name="T">The type of the matrix elements.</typeparam>
	public interface IMat<M, T> : IEquatable<M>
		where M : struct, IMat<M, T>
		where T : struct, IEquatable<T>
	{
		/// <summary>
		/// Add two matrices together elementwise.
		/// </summary>
		M Add (M mat);

		/// <summary>
		/// Subtract a matrix from this one elementwise.
		/// </summary>
		M Subtract (M mat);

		/// <summary>
		/// Multiply the matrix elements by a scalar.
		/// </summary>
		M Multiply (T scalar);

		/// <summary>
		/// Multiply a vector by this matrix. The number of elements in the
		/// vector must be the same as the number of columns in the matrix.
		/// If this is not the case the implementation should throw an
		/// ArgumentException.
		/// </summary>
		V Multiply<V> (V vec) where V : struct, IVec<V, T>, IEquatable<V>;

		/// <summary>
		/// Divide the elements of the matrix by a scalar.
		/// </summary>
		M Divide (T scalar);

		/// <summary>
		/// Return the number of columns in the matrix.
		/// </summary>
		int Columns { get; }

		/// <summary>
		/// Return the number of rows in the matrix.
		/// </summary>
		int Rows { get; }

		/// <summary>
		/// Return the element of the matrix give its row and column.
		/// </summary>
		T this[int column, int row] { get; set; }
	}

	/// <summary>
	/// Interface for square matrices.
	/// </summary>
	/// Square matrices have the same number of rows and columns. They are the most
	/// prevalent matrices in OpenGL applications, since most of the math involving
	/// matrices uses the square matrices.
	/// 
	/// The interface generalizes the matrix structs, and makes it possible for defining 
	/// general operations that work on all square matrices.
	/// <typeparam name="M">The type of the matrix struct implementing this interface.</typeparam>
	/// <typeparam name="T">The type of the matrix elements.</typeparam>
	public interface ISquareMat<M, T> : IMat<M, T>
		where M : struct, ISquareMat<M, T>, IEquatable<M>
		where T : struct, IEquatable<T>
	{
		/// <summary>
		/// Multiply two matrices together. Matrix multiplication combines the affine
		/// transformations that the matrices represent. It is the most common operation
		/// performed on matrices. <see href="https://en.wikipedia.org/wiki/Matrix_multiplication"/>
		/// </summary>
		M Multiply (M mat);

		/// <summary>
		/// Return the transposed matrix with rows and columns swapped.
		/// </summary>
		M Transposed { get; }

		/// <summary>
		/// Return the determinant of the matrix.
		/// </summary>
		T Determinant { get; }

		/// <summary>
		/// Return the matrix inverse. This operation is much slower than the other matrix
		/// operations, so it should be used sparingly on performance critical code.
		/// </summary>
		M Inverse { get; }
	}

	/// <summary>
	/// A static class that defines generic matrix operations. Most of them are defined
	/// as extension methods to make them more discoverable.
	/// </summary>
	public static class Mat
    {
        public static bool ApproxEquals<M> (M mat, M other, float epsilon)
            where M : struct, IMat<M, float>
        {
            for (int c = 0; c < mat.Columns; c++)
                for (int r = 0; r < mat.Rows; r++)
                    if (!mat[c, r].ApproxEquals (other[c, r], epsilon)) return false;
            return true;
        }

        public static bool ApproxEquals<M> (M mat, M other)
            where M : struct, IMat<M, float>
        {
            return ApproxEquals<M> (mat, other, 0.000001f);
        }

        public static T[] ToArray<M, T> (M mat)
            where M : struct, IMat<M, T>
            where T : struct, IEquatable<T>
        {
            var result = new T[mat.Rows * mat.Columns];
            for (int i = 0, c = 0; c < mat.Columns; c++)
                for (int r = 0; r < mat.Rows; r++)
                    result[i++] = mat[c, r];
            return result;
        }

        public static T[][] ToJaggedArray<M, T> (M mat) 
            where M : struct, IMat<M, T>
            where T : struct, IEquatable<T>
        {
            var result = new T[mat.Rows][];
            for (int r = 0; r < mat.Rows; r++)
            {
                result[r] = new T[mat.Columns];
                for (int c = 0; c < mat.Columns; c++)
                    result[r][c] = mat[c, r];
            }
            return result;
        }

        public static M FromArray<M, T> (T[] array)
            where M : struct, IMat<M, T>
            where T : struct, IEquatable<T>
        {
            var result = default (M);
            for (int i = 0, c = 0; c < result.Columns; c++)
                for (int r = 0; r < result.Rows; r++, i = (i + 1) % array.Length)
                    result[c, r] = array[i];
            return result;
        }

        public static M FromJaggedArray<M, T> (T[][] array)
            where M : struct, IMat<M, T>
            where T : struct, IEquatable<T>
        {
            var result = default (M);
            for (int r = 0; r < array.Length; r++)
                for (int c = 0; c < array[r].Length; c++)
                    result[c, r] = array[r][c];
            return result;
        }

        public static N ConvertTo<M, N, T> (this M mat)
            where M : struct, IMat<M, T>
            where N : struct, IMat<N, T>
            where T : struct, IEquatable<T>
        {
            var res = default (N);
            for (int c = 0; c < Math.Min (mat.Columns, res.Columns); c++)
                for (int r = 0; r < Math.Min (mat.Rows, res.Rows); r++)
                    res[c, r] = mat[c, r];
            return res;
        }

        public static M Transpose<M, T> (M mat)
            where M : struct, ISquareMat<M, T>
            where T : struct, IEquatable<T>
        {
            var result = default (M);
            for (int r = 0; r < mat.Rows; r++)
                for (int c = 0; c < mat.Columns; c++)
                    result[r, c] = mat[c, r];
            return result;
        }

        public static M Identity<M> ()
            where M : struct, ISquareMat<M, float>
        {
            var result = default (M);
            for (int c = 0; c < result.Columns; c++)
                result[c, c] = 1f;
            return result;
        }

        public static M Translation<M> (params float[] offsets)
            where M : struct, ISquareMat<M, float>
        {
            var res = Identity<M> ();
            if (res.Rows <= offsets.Length)
                throw new ArgumentOutOfRangeException ("offsets", "Too many offsets.");
            var lastcol = res.Columns - 1;
            for (int i = 0; i < offsets.Length; i++)
                res[lastcol, i] = offsets[i];
            return res;
        }

        public static M Scaling<M> (params float[] factors)
            where M : struct, ISquareMat<M, float>
        {
            var res = Identity<M> ();
            if (factors.Length > res.Columns || factors.Length > res.Rows)
                throw new ArgumentOutOfRangeException ("factors", "Too many factors.");
            for (int i = 0; i < factors.Length; i++)
                res[i, i] = factors[i];
            return res;
        }
		
		public static Mat4 ScalingPerpendicularTo (Vec3 vec, Vec2 factors)
		{
			var rotx = vec.XRotation ();
			var roty = vec.YRotation ();
			return RotationX<Mat4> (rotx) * RotationY<Mat4> (roty) * 
				Scaling<Mat4> (factors.X, factors.Y) * 
				RotationY<Mat4> (-roty) * RotationX<Mat4> (-rotx);
		}

        public static M RotationX<M> (float alpha)
            where M : struct, ISquareMat<M, float>
        {
            var res = Identity<M> ();
            var sina = (float)Math.Sin (alpha);
            var cosa = (float)Math.Cos (alpha);
            res[1, 1] = cosa;
            res[1, 2] = sina;
            res[2, 1] = -sina;
            res[2, 2] = cosa;
            return res;
        }

        public static M RotationY<M> (float alpha)
            where M : struct, ISquareMat<M, float>
        {
            var res = Identity<M> ();
            var sina = (float)Math.Sin (alpha);
            var cosa = (float)Math.Cos (alpha);
            res[0, 0] = cosa;
            res[0, 2] = -sina;
            res[2, 0] = sina;
            res[2, 2] = cosa;
            return res;
        }

        public static M RotationZ<M> (float alpha)
            where M : struct, ISquareMat<M, float>
        {
            var res = Identity<M> ();
            var sina = (float)Math.Sin (alpha);
            var cosa = (float)Math.Cos (alpha);
            res[0, 0] = cosa;
            res[0, 1] = sina;
            res[1, 0] = -sina;
            res[1, 1] = cosa;
            return res;
        }

		public static M RelativeTo<M, V> (this M mat, V vec)
			where M : struct, ISquareMat<M, float>
			where V : struct, IVec<V, float>
		{
			return Translation<M> (vec.ToArray<V, float> ())
				.Multiply (mat)
				.Multiply (Translation<M> (vec.Multiply (-1f).ToArray<V, float> ()));
		}

		public static M RemoveTranslation<M> (this M mat)
			where M : struct, ISquareMat<M, float>
		{
			var lastcol = mat.Columns - 1;
			for (int i = 0; i < mat.Rows - 1; i++)
				mat[lastcol, i] = 0f;
			return mat;
		}

        public static float Determinant<M> (M mat)
            where M : struct, ISquareMat<M, float>
        {
            return DeterminantFA (ToJaggedArray<M, float> (mat));
        }

        public static M Inverse<M> (M mat)
            where M : struct, ISquareMat<M, float>
        {
            return FromJaggedArray<M, float> (InverseFA (ToJaggedArray<M, float> (mat)));
        }

        public static Mat4 PerspectiveProjection (float left, float right, float bottom, float top,
            float zNear, float zFar)
        {
            if (zNear <= 0 || zNear >= zFar)
                throw new ArgumentOutOfRangeException ("zNear");
            var width = right - left;
            var height = top - bottom;
            var depth = zFar - zNear;

            return new Mat4 (
                new Vec4 ((2.0f * zNear) / width, 0f, 0f, 0f ),
                new Vec4 (0f, (2.0f * zNear) / height, 0f, 0f),
                new Vec4 ((right + left) / width, (top + bottom) / height, -(zFar + zNear) / depth, -1f),
                new Vec4 (0f, 0f, -(2.0f * zFar * zNear) / depth, 0f));
        }

        public static Mat4 OrthographicProjection (float left, float right, float bottom, float top,
            float zNear, float zFar)
        {
            float invWidth = 1.0f / (right - left);
            float invHeight = 1.0f / (top - bottom);
            float invDepth = 1.0f / (zFar - zNear);

            return new Mat4 (
                new Vec4 (2f * invWidth, 0f, 0f, 0f),
                new Vec4 (0f, 2f * invHeight, 0f, 0f),
                new Vec4 (0f, 0f, -2f * invDepth, 0f),
                new Vec4 (-(right + left) * invWidth, -(top + bottom) * invHeight, -(zFar + zNear) * invDepth, 1f));
        }

		public static Mat4 LookAt (Vec3 direction, Vec3 up)
		{
			var zaxis = -direction.Normalized;
			var xaxis = up.Cross (zaxis).Normalized;
			var yaxis = zaxis.Cross (xaxis);

			return new Mat4 (
				xaxis.X, yaxis.X, zaxis.X, 0f,
				xaxis.Y, yaxis.Y, zaxis.Y, 0f,
				xaxis.Z, yaxis.Z, zaxis.Z, 0f,
				0f, 0f, 0f, 1f);
		}

		public static Mat4 LookAt (Vec3 eye, Vec3 target, Vec3 up)
		{
			var zaxis = (eye - target).Normalized;
			var xaxis = up.Cross (zaxis).Normalized;
			var yaxis = zaxis.Cross (xaxis);

			return new Mat4 (
				xaxis.X, yaxis.X, zaxis.X, 0f,
				xaxis.Y, yaxis.Y, zaxis.Y, 0f,
				xaxis.Z, yaxis.Z, zaxis.Z, 0f,
				-xaxis.Dot (eye), -yaxis.Dot (eye), -zaxis.Dot (eye), 1f);
		}

		public static V Transform<V> (this Mat4 mat, V point)
			where V : struct, IVec<V, float>
        {
			var v = new Vec4 (1f);
			for (int i = 0; i < point.Dimensions; i++)
				v [i] = point [i];
			v = mat * v;
			return Vec.FromArray<V, float> (v.X, v.Y, v.Z, v.W);
        }

        /// <summary>
        /// Doolittle LUP decomposition with partial pivoting. 
        /// </summary>
        /// <param name="matrix">The matrix to be decomposed in-place.</param>
        /// <returns>Result is L (with 1s on diagonal) and U; perm holds row permutations; 
        /// toggle is +1 or -1 (even or odd)</returns>
        public static Tuple<int[], int> DecomposeFA (float[][] matrix)
        {
            int rows = matrix.Length;
            int cols = matrix[0].Length;
            if (rows != cols)
                throw new ArgumentException ("Cannot decompose non-square matrix", "matrix");
            // set up row permutation result
            var perm = new int[rows];
            for (int i = 0; i < rows; ++i) 
                perm[i] = i;
            // toggle tracks row swaps. +1 -> even, -1 -> odd. used by MatrixDeterminant
            var toggle = 1; 
            for (int c = 0; c < cols - 1; ++c) // each column
            {
                float colMax = Math.Abs (matrix[c][c]); // find largest value in col j
                int pRow = c;
                for (int r = c + 1; r < rows; ++r)
                    if (matrix[r][c] > colMax)
                    {
                        colMax = matrix[r][c];
                        pRow = r;
                    }
                if (pRow != c) 
                {
                    // if largest value not on pivot, swap rows
                    var rowPtr = matrix[pRow];
                    matrix[pRow] = matrix[c];
                    matrix[c] = rowPtr;
                    // and swap perm info
                    int tmp = perm[pRow];
                    perm[pRow] = perm[c];
                    perm[c] = tmp;
                    // adjust the row-swap toggle
                    toggle = -toggle;                 
                }
				if (matrix[c][c] == 0f)
					matrix[c][c] = 0.000001f;
					//throw new ArgumentException ("Matrix is singular", "matrix");
                for (int r = c + 1; r < rows; ++r)
                {
                    matrix[r][c] /= matrix[c][c];
                    for (int k = c + 1; k < cols; ++k)
                        matrix[r][k] -= matrix[r][c] * matrix[c][k];
                }
            }
            return Tuple.Create (perm, toggle);
        }

        /// <summary>
        /// Returns the determinant of matrix.
        /// </summary>
        /// <param name="matrix">The input matrix is decomposed in-place.</param>
        /// <returns>The determinant of matrix.</returns>
        public static float DeterminantFA (float[][] matrix)
        {
            var perm_toggle = DecomposeFA (matrix);
            float result = perm_toggle.Item2;
            for (int i = 0; i < matrix.Length; ++i)
                result *= matrix[i][i];
            return result;
        }

        public static float[][] InverseFA (float[][] matrix)
        {
            int rows = matrix.Length;
            var result = matrix.Copy ();
            var perm_toggle = DecomposeFA (matrix);
            var b = new float[rows];

            for (int c = 0; c < rows; ++c)
            {
                for (int r = 0; r < rows; ++r)
                    b[r] = c == perm_toggle.Item1[r] ? 1f : 0f;
                var x = HelperSolvef (matrix, b); 
                for (int r = 0; r < rows; ++r)
                    result[r][c] = x[r];
            }
            return result;
        }

        private static float[] HelperSolvef (float[][] luMatrix, float[] vector)
        {
            // before calling this helper, permute b using the perm array from 
            // MatrixDecompose that generated luMatrix
            int rows = luMatrix.Length;
            var res = new float[rows];
            Array.Copy (vector, res, rows);

            for (int r = 1; r < rows; ++r)
            {
                var sum = res[r];
                for (int c = 0; c < r; ++c)
                    sum -= luMatrix[r][c] * res[c];
                res[r] = sum;
            }

            res[rows - 1] /= luMatrix[rows - 1][rows - 1];
            for (int r = rows - 2; r >= 0; --r)
            {
                var sum = res[r];
                for (int c = r + 1; c < rows; ++c)
                    sum -= luMatrix[r][c] * res[c];
                res[r] = sum / luMatrix[r][r];
            }
            return res;
        }
    } 
}

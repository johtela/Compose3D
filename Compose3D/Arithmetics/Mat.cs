﻿namespace Compose3D.Arithmetics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public static class Mat
    {
        public static bool ApproxEquals<M> (M mat, M other)
            where M : struct, IMat<M, float>
        {
            for (int c = 0; c < mat.Columns; c++)
                for (int r = 0; r < mat.Rows; r++)
                    if (!mat[c, r].ApproxEquals (other[c, r])) return false;
            return true;
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

        public static Mat4 PerspectiveOffCenter (float left, float right, float bottom, float top,
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
                if (Math.Abs (matrix[c][c]) != 0f)
                    throw new ArgumentException ("Matrix is singular", "matrix");
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
            var result = matrix.Duplicate ();
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

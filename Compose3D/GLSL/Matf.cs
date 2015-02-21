using System;

namespace Compose3D.GLSL
{
    public static class Matf
    {
        public static M Add<M> ( M left, M right) where M : Mat<float>, new ()
        {
            return left.MapWith<M, float> (right, (a, b) => a + b);
        }

        public static M Multiply<M> (M mat, float scalar) where M : Mat<float>, new ()
        {
            return mat.Map<M, float> (a => a * scalar);
        }

        public static M Multiply<M> (M left, M right) where M : Mat<float>, new ()
        {
            return left.Multiply<M, float> (right, (s, a, b) => s + a * b);
        }

        public static V Multiply<M, V> (M mat, V vec)
            where M : Mat<float>
            where V : Vec<float>, new ()    
        {
            if (mat.Rows != vec.Vector.Length)
                throw new ArgumentException (string.Format (
                    "Cannot multiply {0}x{1} matrix with {2}-component vector", 
                    mat.Columns, mat.Rows, vec.Vector.Length));
            var result = new V ();
            mat.Matrix.Multiply (vec.Vector, result.Vector, (s, a, b) => s + a * b);
            return result;
        }

        public static M Subtract<M> (M left, M right) where M : Mat<float>, new ()
        {
            return left.MapWith<M, float> (right, (a, b) => a - b);
        }

        public static M Identity<M> () where M : Mat<float>, new ()
        {
            var res = new M ();
            for (int i = 0; i < Math.Min (res.Columns, res.Rows); i++)
                res[i, i] = 1f;
            return res;
        }

        public static M Translation<M> (params float[] offsets) where M : Mat<float>, new ()
        {
            var res = Identity<M> ();
            var lastrow = res.Rows - 1;
            for (int i = 0; i < offsets.Length; i++)
                res[i, lastrow] = offsets[i];
            return res;
        }

        public static M Scaling<M> (params float[] factors) where M : Mat<float>, new ()
        {
            var res = Identity<M> ();
            for (int i = 0; i < factors.Length; i++)
                res[i, i] = factors[i];
            return res;
        }

        public static M RotationX<M> (float alpha) where M : Mat<float>, new ()
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

        public static M RotationY<M> (float alpha) where M : Mat<float>, new ()
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

        public static M RotationZ<M> (float alpha) where M : Mat<float>, new ()
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

        //public static M Invert<M> (this M mat) where M : Mat<float>, new()
        //{
        //    int[] colIdx = { 0, 0, 0 };
        //    int[] rowIdx = { 0, 0, 0 };
        //    int[] pivotIdx = { -1, -1, -1 };

        //    float[,] inverse = {{mat.Row0.X, mat.Row0.Y, mat.Row0.Z},
        //        {mat.Row1.X, mat.Row1.Y, mat.Row1.Z},
        //        {mat.Row2.X, mat.Row2.Y, mat.Row2.Z}};

        //    int icol = 0;
        //    int irow = 0;
        //    for (int i = 0; i < 3; i++)
        //    {
        //        float maxPivot = 0.0f;
        //        for (int j = 0; j < 3; j++)
        //        {
        //            if (pivotIdx[j] != 0)
        //            {
        //                for (int k = 0; k < 3; ++k)
        //                {
        //                    if (pivotIdx[k] == -1)
        //                    {
        //                        float absVal = System.Math.Abs (inverse[j, k]);
        //                        if (absVal > maxPivot)
        //                        {
        //                            maxPivot = absVal;
        //                            irow = j;
        //                            icol = k;
        //                        }
        //                    }
        //                    else if (pivotIdx[k] > 0)
        //                    {
        //                        result = mat;
        //                        return;
        //                    }
        //                }
        //            }
        //        }

        //        ++(pivotIdx[icol]);

        //        if (irow != icol)
        //        {
        //            for (int k = 0; k < 3; ++k)
        //            {
        //                float f = inverse[irow, k];
        //                inverse[irow, k] = inverse[icol, k];
        //                inverse[icol, k] = f;
        //            }
        //        }

        //        rowIdx[i] = irow;
        //        colIdx[i] = icol;

        //        float pivot = inverse[icol, icol];

        //        if (pivot == 0.0f)
        //        {
        //            throw new InvalidOperationException ("Matrix is singular and cannot be inverted.");
        //        }

        //        float oneOverPivot = 1.0f / pivot;
        //        inverse[icol, icol] = 1.0f;
        //        for (int k = 0; k < 3; ++k)
        //            inverse[icol, k] *= oneOverPivot;

        //        for (int j = 0; j < 3; ++j)
        //        {
        //            if (icol != j)
        //            {
        //                float f = inverse[j, icol];
        //                inverse[j, icol] = 0.0f;
        //                for (int k = 0; k < 3; ++k)
        //                    inverse[j, k] -= inverse[icol, k] * f;
        //            }
        //        }
        //    }

        //    for (int j = 2; j >= 0; --j)
        //    {
        //        int ir = rowIdx[j];
        //        int ic = colIdx[j];
        //        for (int k = 0; k < 3; ++k)
        //        {
        //            float f = inverse[k, ir];
        //            inverse[k, ir] = inverse[k, ic];
        //            inverse[k, ic] = f;
        //        }
        //    }

        //    result.Row0.X = inverse[0, 0];
        //    result.Row0.Y = inverse[0, 1];
        //    result.Row0.Z = inverse[0, 2];
        //    result.Row1.X = inverse[1, 0];
        //    result.Row1.Y = inverse[1, 1];
        //    result.Row1.Z = inverse[1, 2];
        //    result.Row2.X = inverse[2, 0];
        //    result.Row2.Y = inverse[2, 1];
        //    result.Row2.Z = inverse[2, 2];
        //}
    }
}
using System;

namespace Compose3D.GLSL
{
    public static class Matf
    {
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
    }
}
using System;

namespace Compose3D.GLSL
{
    public static class Matd
    {
        public static M Identity<M> () where M : Mat<double>, new ()
        {
            var res = new M ();
            for (int i = 0; i < Math.Min (res.Columns, res.Rows); i++)
                res[i, i] = 1.0;
            return res;
        }

        public static M RotationX<M> (double alpha) where M : Mat<double>, new ()
        {
            var res = Identity<M> ();
            var sina = Math.Sin (alpha);
            var cosa = Math.Cos (alpha);
            res[1, 1] = cosa;
            res[1, 2] = sina;
            res[2, 1] = -sina;
            res[2, 2] = cosa;
            return res;
        }

        public static M RotationY<M> (double alpha) where M : Mat<double>, new ()
        {
            var res = Identity<M> ();
            var sina = Math.Sin (alpha);
            var cosa = Math.Cos (alpha);
            res[0, 0] = cosa;
            res[0, 2] = -sina;
            res[2, 0] = sina;
            res[2, 2] = cosa;
            return res;
        }

        public static M RotationZ<M> (double alpha) where M : Mat<double>, new ()
        {
            var res = Identity<M> ();
            var sina = Math.Sin (alpha);
            var cosa = Math.Cos (alpha);
            res[0, 0] = cosa;
            res[0, 1] = sina;
            res[1, 0] = -sina;
            res[1, 1] = cosa;
            return res;
        }
    }
}

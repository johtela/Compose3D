namespace ComposeTester
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Compose3D;
    using Compose3D.GLSL;
    using LinqCheck;

    public class MatTests
    {
        static MatTests ()
        {
            Arbitrary.Register (ArbitraryMat<Mat2, float> (2, 2));
            Arbitrary.Register (ArbitraryMat<Mat3, float> (3, 3));
            Arbitrary.Register (ArbitraryMat<Mat4, float> (4, 4));
        }

        public static Arbitrary<M> ArbitraryMat<M, T> (int cols, int rows) 
            where M : Mat<T>, new ()
            where T : struct, IEquatable<T>
        {
            var arb = Arbitrary.Get<T> (); 
            return new Arbitrary<M> ( 
                from a in arb.Generate.FixedArrayOf (cols * rows)
                select Mat<T>.Create<M> (a),
                v => from a in v.ToArray ().Combinations (arb.Shrink)
                     select Mat<T>.Create<M> (a));
        }

        public void CheckAddSubtract<M> () where M : Mat<float>, new ()
        {
            var prop = from mat1 in Prop.Choose<M> ()
                       from mat2 in Prop.Choose<M> ()
                       let neg = Matf.Negate (mat2)
                       select new { mat1, mat2, neg };

            prop.Label ("{0}: mat1 - mat1 = [ 0 ... ]", typeof (M).Name)
                .Check (p => Matf.Subtract (p.mat1, p.mat1) == new M ());
            prop.Label ("{0}: mat1 - mat2 = mat1 + (-mat2)", typeof (M).Name)
                .Check (p => Matf.Subtract (p.mat1, p.mat2) == Matf.Add (p.mat1, p.neg));
        }

        [Test]
        public void TestAddSubtract ()
        {
            CheckAddSubtract<Mat2> ();
            CheckAddSubtract<Mat3> ();
            CheckAddSubtract<Mat4> ();
        }
    }
}

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

        public void CheckMultiplyScalar<M> () where M : Mat<float>, new ()
        {
            var prop = from mat in Prop.Choose<M> ()
                       from scalar in Prop.Choose<float> ()
                       let mult = Matf.MultiplyScalar (mat, scalar)
                       let multdiv = Matf.MultiplyScalar (mult, 1 / scalar)
                       select new { mat, scalar, mult, multdiv };

            prop.Label ("{0}: (mat * scalar) * (1 / scalar) = mat", typeof (M).Name)
                .Check (p => p.mat.ApproxEquals (p.multdiv));
        }

        public void CheckTranspose<M> () where M : Mat<float>, new ()
        {
            var prop = from mat1 in Prop.Choose<M> ()
                       from mat2 in Prop.Choose<M> ()
                       let mat1t = mat1.Transpose<M, float> ()
                       let mat1tt = mat1t.Transpose<M, float> ()
                       let mat2t = mat2.Transpose<M, float> ()
                       let add_mat1t_mat2t = Matf.Add (mat1t, mat2t)
                       let addt_mat1_mat2 = Matf.Add (mat1, mat2).Transpose<M, float> ()
                       select new { mat1, mat1t, mat1tt, mat2, mat2t, add_mat1t_mat2t, addt_mat1_mat2 };

            prop.Label ("{0}: mat.Rows = mat^T.Columns and mat.Columns = mat^T.Rows", typeof (M).Name)
                .Check (p => p.mat1.Rows == p.mat1t.Columns && p.mat1.Columns == p.mat1t.Rows);
            prop.Label ("{0}: mat^TT = mat", typeof (M).Name)
                .Check (p => p.mat1 == p.mat1tt);
            prop.Label ("{0}: mat1^T + mat2^T = (mat1 + mat2)^T", typeof (M).Name)
                .Check (p => p.add_mat1t_mat2t == p.addt_mat1_mat2);
        }

        public void CheckMultiplyMatrices<M> () where M : Mat<float>, new ()
        {
            var prop = from mat1 in Prop.Choose<M> ()
                       from mat2 in Prop.Choose<M> ()
                       from mat3 in Prop.Choose<M> ()
                       let ident = Matf.Identity<M> ()
                       let mult_mat1_ident = Matf.Multiply (mat1, ident)
                       let mult_mat12 = Matf.Multiply (mat1, mat2)
                       let mult_mat12_3 = Matf.Multiply (mult_mat12, mat3)
                       let mult_mat23 = Matf.Multiply (mat2, mat3)
                       let mult_mat1_23 = Matf.Multiply (mat1, mult_mat23)
                       select new { mat1, mat2, ident, mult_mat1_ident,
                           mult_mat12, mult_mat12_3, mult_mat23, mult_mat1_23 };

            prop.Label ("{0}: mat * I = mat", typeof (M).Name)
                .Check (p => p.mult_mat1_ident == p.mat1);
            prop.Label ("{0}: (mat1 * mat2) * mat3 = mat1 * (mat2 * mat3)", typeof (M).Name)
                .Check (p => p.mult_mat12_3.ApproxEquals (p.mult_mat1_23));
        }

        public void CheckTranslation<M, V> () 
            where M : Mat<float>, new ()
            where V : Vec<float>, new ()
        {
            var prop = from v in Prop.Choose<V> ()
                       from o in Prop.Choose<V> ()
                       let last = v.Vector.Length - 1
                       let vec = v.With (last, 1f)
                       let offset = v.With (last, 0f)
                       let trans = Matf.Translation<M> (offset.Vector.Segment (0, last))
                       let transvec = Matf.MultiplyVector (trans, vec)
                       select new { vec, offset, trans, transvec };

            prop.Label ("{0}, {1}: trans * vec = vec + offset", typeof (M).Name, typeof (V).Name)
                .Check (p => p.transvec == Vecf.Add (p.vec, p.offset));
        }

        public void CheckScaling<M, V> ()
            where M : Mat<float>, new ()
            where V : Vec<float>, new ()
        {
            var prop = from vec in Prop.Choose<V> ()
                       from scale in Prop.Choose<V> ()
                       let trans = Matf.Scaling<M> (scale.Vector)
                       let transvec = Matf.MultiplyVector (trans, vec)
                       select new { vec, scale, trans, transvec };

            prop.Label ("{0}, {1}: trans * vec = vec * scale", typeof (M).Name, typeof (V).Name)
                .Check (p => p.transvec == Vecf.Multiply (p.vec, p.scale));
        }

        public void CheckRotationZ<M, V> ()
            where M : Mat<float>, new ()
            where V : Vec<float>, new ()
        {
            var prop = from vec in Prop.Choose<V> ()
                       from rot in Prop.Choose<float> ()
                       let trans = Matf.RotationZ<M> (rot)
                       let transvec = Matf.MultiplyVector (trans, vec)
                       select new { vec, rot, trans, transvec };

            prop.Label ("{0}, {1}: | trans * vec | = | vec |", typeof (M).Name, typeof (V).Name)
                .Check (p => p.transvec.Length ().ApproxEquals (p.vec.Length ()));
        }

        public void CheckRotationXY<M, V> ()
            where M : Mat<float>, new ()
            where V : Vec<float>, new ()
        {
            var prop = from vec in Prop.Choose<V> ()
                       from rotX in Prop.Choose<float> ()
                       from rotY in Prop.Choose<float> ()
                       let trans = Matf.Multiply (Matf.RotationX<M> (rotX), Matf.RotationY<M> (rotY))
                       let transvec = Matf.MultiplyVector (trans, vec)
                       select new { vec, rotX, rotY, trans, transvec };

            prop.Label ("{0}, {1}: | trans * vec | = | vec |", typeof (M).Name, typeof (V).Name)
                .Check (p => p.transvec.Length ().ApproxEquals (p.vec.Length ()));
        }

        [Test]
        public void TestAddSubtract ()
        {
            CheckAddSubtract<Mat2> ();
            CheckAddSubtract<Mat3> ();
            CheckAddSubtract<Mat4> ();
        }

        [Test]
        public void TestMultiplyScalar ()
        {
            CheckMultiplyScalar<Mat2> ();
            CheckMultiplyScalar<Mat3> ();
            CheckMultiplyScalar<Mat4> ();
        }

        [Test]
        public void TestTranspose ()
        {
            CheckTranspose<Mat2> ();
            CheckTranspose<Mat3> ();
            CheckTranspose<Mat4> ();
        }

        [Test]
        public void TestMultiplyMatrices ()
        {
            CheckMultiplyMatrices<Mat2> ();
            CheckMultiplyMatrices<Mat3> ();
            CheckMultiplyMatrices<Mat4> ();
        }

        [Test]
        public void TestTranslation ()
        {
            CheckTranslation<Mat2, Vec2> ();
            CheckTranslation<Mat3, Vec3> ();
            CheckTranslation<Mat4, Vec4> ();
        }

        [Test]
        public void TestScaling ()
        {
            CheckScaling<Mat2, Vec2> ();
            CheckScaling<Mat3, Vec3> ();
            CheckScaling<Mat4, Vec4> ();
        }

        [Test]
        public void TestRotation ()
        {
            CheckRotationZ<Mat2, Vec2> ();
            CheckRotationZ<Mat3, Vec3> ();
            CheckRotationZ<Mat4, Vec4> ();
            CheckRotationXY<Mat3, Vec3> ();
            CheckRotationXY<Mat4, Vec4> ();
        }
    }
}

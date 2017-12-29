namespace ComposeTester
{
	using Compose3D.Maths;
	using Extensions;
	using LinqCheck;
	using System;
	using System.Linq;

	public class MatTests
    {
        static MatTests ()
        {
            Arbitrary.Register (ArbitraryMat<Mat2, float> (2, 2));
            Arbitrary.Register (ArbitraryMat<Mat3, float> (3, 3));
            Arbitrary.Register (ArbitraryMat<Mat4, float> (4, 4));
        }

        public static Arbitrary<M> ArbitraryMat<M, T> (int cols, int rows) 
            where M : struct, IMat<M, T>
            where T : struct, IEquatable<T>
        {
            var arb = Arbitrary.Get<T> (); 
            return new Arbitrary<M> ( 
                from a in arb.Generate.FixedArrayOf (rows * cols)
                select Mat.FromArray<M, T> (a),
                m => from a in Mat.ToArray<M, T> (m).Map (arb.Shrink).Combinations ()
                     select Mat.FromArray<M, T> (a));
        }

        public void CheckAddSubtract<M> () where M : struct, IMat<M, float>
        {
            var prop = from mat1 in Prop.Choose<M> ()
                       from mat2 in Prop.Choose<M> ()
                       let neg = mat2.Multiply (-1f)
                       select new { mat1, mat2, neg };

            prop.Label ("{0}: mat1 - mat1 = [ 0 ... ]", typeof (M).Name)
                .Check (p => p.mat1.Subtract (p.mat1).Equals (default (M)));
            prop.Label ("{0}: mat1 - mat2 = mat1 + (-mat2)", typeof (M).Name)
                .Check (p => p.mat1.Subtract (p.mat2).Equals (p.mat1.Add (p.neg)));
        }

        public void CheckMultiplyScalar<M> () where M : struct, IMat<M, float>
        {
            var prop = from mat in Prop.Choose<M> ()
                       from scalar in Prop.Choose<float> ()
                       let mult = mat.Multiply (scalar)
                       let multdiv = mult.Multiply (1 / scalar)
                       select new { mat, scalar, mult, multdiv };

            prop.Label ("{0}: (mat * scalar) * (1 / scalar) = mat", typeof (M).Name)
                .Check (p => Mat.ApproxEquals (p.mat, p.multdiv));
        }

        public void CheckTranspose<M> () where M : struct, ISquareMat<M, float>
        {
            var prop = from mat1 in Prop.Choose<M> ()
                       from mat2 in Prop.Choose<M> ()
                       let mat1t = mat1.Transposed
                       let mat1tt = mat1t.Transposed
                       let mat2t = mat2.Transposed
                       let add_mat1t_mat2t = mat1t.Add (mat2t)
                       let addt_mat1_mat2 = mat1.Add (mat2).Transposed
                       select new { mat1, mat1t, mat1tt, mat2, mat2t, add_mat1t_mat2t, addt_mat1_mat2 };

            prop.Label ("{0}: mat.Rows = mat^T.Columns and mat.Columns = mat^T.Rows", typeof (M).Name)
                .Check (p => p.mat1.Rows == p.mat1t.Columns && p.mat1.Columns == p.mat1t.Rows);
            prop.Label ("{0}: mat^TT = mat", typeof (M).Name)
                .Check (p => p.mat1.Equals (p.mat1tt));
            prop.Label ("{0}: mat1^T + mat2^T = (mat1 + mat2)^T", typeof (M).Name)
                .Check (p => p.add_mat1t_mat2t.Equals (p.addt_mat1_mat2));
        }

        public void CheckMultiplyMatrices<M> () where M : struct, ISquareMat<M, float>
        {
            var prop = from mat1 in Prop.Choose<M> ()
                       from mat2 in Prop.Choose<M> ()
                       from mat3 in Prop.Choose<M> ()
                       let ident = Mat.Identity<M> ()
                       let mult_mat1_ident = mat1.Multiply (ident)
                       let mult_mat12 = mat1.Multiply (mat2)
                       let mult_mat12_3 = mult_mat12.Multiply (mat3)
                       let mult_mat23 = mat2.Multiply (mat3)
                       let mult_mat1_23 = mat1.Multiply (mult_mat23)
                       select new { mat1, mat2, ident, mult_mat1_ident,
                           mult_mat12, mult_mat12_3, mult_mat23, mult_mat1_23 };

            prop.Label ("{0}: mat * I = mat", typeof (M).Name)
                .Check (p => p.mult_mat1_ident.Equals (p.mat1));
            prop.Label ("{0}: (mat1 * mat2) * mat3 = mat1 * (mat2 * mat3)", typeof (M).Name)
                .Check (p => Mat.ApproxEquals (p.mult_mat12_3, p.mult_mat1_23, 0.001f));
        }

        public void CheckTranslation<M, V> () 
            where M : struct, ISquareMat<M, float>
            where V : struct, IVec<V, float>
        {
            var prop = from v in Prop.Choose<V> ()
                       from o in Prop.Choose<V> ()
                       let last = v.Dimensions - 1
                       let vec = v.With (last, 1f)
                       let offset = v.With (last, 0f)
                       let trans = Mat.Translation<M> (offset.ToArray<V, float> ().Segment (0, last))
                       let transvec = trans.Multiply (vec)
                       select new { vec, offset, trans, transvec };

            prop.Label ("{0}, {1}: trans * vec = vec + offset", typeof (M).Name, typeof (V).Name)
                .Check (p => p.transvec.Equals (p.vec.Add (p.offset)));
        }

        public void CheckScaling<M, V> ()
            where M : struct, ISquareMat<M, float>
            where V : struct, IVec<V, float>
        {
            var prop = from vec in Prop.Choose<V> ()
                       from scale in Prop.Choose<V> ()
                       let trans = Mat.Scaling<M> (scale.ToArray<V, float> ())
                       let transvec = trans.Multiply (vec)
                       select new { vec, scale, trans, transvec };

            prop.Label ("{0}, {1}: trans * vec = vec * scale", typeof (M).Name, typeof (V).Name)
                .Check (p => p.transvec.Equals (p.vec.Multiply (p.scale)));
        }

        public void CheckRotationZ<M, V> ()
            where M : struct, ISquareMat<M, float>
            where V : struct, IVec<V, float>
        {
            var prop = from vec in Prop.Choose<V> ()
                       from rot in Prop.Choose<float> ()
                       let trans = Mat.RotationZ<M> (rot)
                       let transvec = trans.Multiply (vec)
                       select new { vec, rot, trans, transvec };

            prop.Label ("{0}, {1}: | trans * vec | = | vec |", typeof (M).Name, typeof (V).Name)
                .Check (p => p.transvec.Length.ApproxEquals (p.vec.Length));
        }

        public void CheckRotationXY<M, V> ()
            where M : struct, ISquareMat<M, float>
            where V : struct, IVec<V, float>
        {
            var prop = from vec in Prop.Choose<V> ()
                       from rotX in Prop.Choose<float> ()
                       from rotY in Prop.Choose<float> ()
                       let trans = Mat.RotationX<M> (rotX).Multiply (Mat.RotationY<M> (rotY))
                       let transvec = trans.Multiply (vec)
                       select new { vec, rotX, rotY, trans, transvec };

            prop.Label ("{0}, {1}: | trans * vec | = | vec |", typeof (M).Name, typeof (V).Name)
                .Check (p => p.transvec.Length.ApproxEquals (p.vec.Length));
        }

        public void CheckInverse<M> ()
            where M : struct, ISquareMat<M, float>
        {
            var prop = from mat in Prop.Choose<M> ()
                       let inv = Mat.Inverse (mat)
                       let mat_inv = mat.Multiply (inv)
                       let ident = Mat.Identity<M> ()
                       select new { mat, inv, mat_inv, ident };

            prop.Label ("{0}: mat * mat^-1 = I", typeof (M).Name)
                .Check (p => Mat.ApproxEquals (p.mat_inv, p.ident, 0.1f));
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

        [Test]
        public void TestInverse ()
        {
            CheckInverse<Mat2> ();
            CheckInverse<Mat3> ();
            CheckInverse<Mat4> ();
        }
    }
}

﻿namespace ComposeTester
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Extensions;
    using Compose3D.Maths;
    using LinqCheck;

    public class VecTests
    {
        static VecTests ()
        {
			Arbitrary.Register (ArbitraryVec<Vec2, float> (2));
			Arbitrary.Register (ArbitraryVec<Vec3, float> (3));
			Arbitrary.Register (ArbitraryVec<Vec4, float> (4));
		}

		public static void Use () {	}

        public static Arbitrary<V> ArbitraryVec<V, T> (int size) 
            where V : struct, IVec<V, T>
            where T : struct, IEquatable<T>
        {
            var arb = Arbitrary.Get<T> (); 
            return new Arbitrary<V> ( 
                from a in arb.Generate.FixedArrayOf (size)
                select Vec.FromArray<V, T> (a),
                v => from a in v.ToArray<V, T> ().Map (arb.Shrink).Combinations ()
                     select Vec.FromArray<V, T> (a));
        }

        public void CheckAddSubtract<V> () where V : struct, IVec<V, float>
        {
            var prop = from vec1 in Prop.Choose<V> ()
                       from vec2 in Prop.Choose<V> ()
                       let neg = vec2.Invert ()
                       select new { vec1, vec2, neg };

            prop.Label ("{0}: vec1 - vec1 = [ 0 ... ]", typeof(V).Name)
                .Check (p => p.vec1.Subtract (p.vec1).Equals (new V ()));
            prop.Label ("{0}: vec1 - vec2 = vec1 + (-vec2)", typeof (V).Name)
                .Check (p => p.vec1.Subtract (p.vec2).Equals (p.vec1.Add (p.neg)));
            prop.Label ("{0}: | vec1 + vec1 | = 2 * | vec1 |", typeof (V).Name)
                .Check (p => p.vec1.Add (p.vec1).Length == p.vec1.Length * 2f);
        }

        public void CheckMultiplyWithScalar<V> () where V : struct, IVec<V, float>
        {
            var prop = from vec in Prop.Choose<V> ()
                       from scalar in Prop.Choose<float> ()
                       let len = vec.Length
                       let scaled = vec.Multiply (scalar)
                       let len_scaled = scaled.Length
                       let scalar_x_len = FMath.Abs (scalar * len)
                       select new { vec, scalar, len, scaled, len_scaled, scalar_x_len };

            prop.Label ("{0}: | vec * scalar | = scalar * | vec |", typeof(V).Name)
                .Check (p => p.len_scaled.ApproxEquals (p.scalar_x_len));
        }

        public void CheckMultiplyWithVector<V> () where V : struct, IVec<V, float>
        {
            var prop = from vec in Prop.Choose<V> ()
                       from scalar in Prop.Choose<float> ()
                       let len = vec.Length
                       let scaleVec = Vec.FromArray<V, float> (scalar.Duplicate (vec.Dimensions))
                       let scaled = vec.Multiply (scaleVec)
                       let len_scaled = scaled.Length
                       let scalar_x_len = FMath.Abs (scaleVec[0] * len)
                       select new { vec, scaleVec, len, scaled, len_scaled, scalar_x_len };

            prop.Label ("{0}: | vec * scale | = scale.x * | vec | when scale.xyzw are equal", typeof (V).Name)
                .Check (p => p.len_scaled.ApproxEquals (p.scalar_x_len));
        }

        public void CheckDivide<V> () where V : struct, IVec<V, float>
        {
            var prop = from vec in Prop.Choose<V> ()
                       from scalar in Prop.Choose<float> ()
                       let divided = vec.Divide (scalar)
                       let multiplied = vec.Multiply (1f / scalar)
                       select new { vec, scalar, divided, multiplied };

            prop.Label ("{0}: vec / scalar = vec * (1 / scalar)", typeof (V).Name)
                .Check (p => Vec.ApproxEquals (p.divided, p.multiplied));
        }

        public void CheckNormalize<V> () where V : struct, IVec<V, float>
        {
            var prop = from vec in Prop.Choose<V> ()
                       let vec_n = vec.Normalized
                       let len = vec_n.Length
                       select new { vec, vec_n, len };

            prop.Label ("{0}: | vec_n | = 1", typeof (V).Name)
                .Check (p => p.len.ApproxEquals (1f));
        }

        public void CheckDotProduct<V> () where V : struct, IVec<V, float>
        {
            var prop = from vec1 in Prop.Choose<V> ()
                       from vec2 in Prop.Choose<V> ()
                       let len_vec1 = vec1.Length
                       let len_vec2 = vec2.Length
                       let vec1n = vec1.Normalized
                       let vec2n = vec2.Normalized
                       let dot_vec1_vec2 = vec1.Dot (vec2)
                       let dot_vec1n_vec2n = vec1n.Dot (vec2n)
                       let dot_vec1_vec2n = vec1.Dot (vec2n)
                       let dot_vec2_vec1n = vec2.Dot (vec1n)
                       select new { vec1, vec2, len_vec1, len_vec2, vec1n, vec2n, 
                           dot_vec1_vec2, dot_vec1n_vec2n, dot_vec1_vec2n, dot_vec2_vec1n };

            prop.Label ("{0}: -1 <= vec1_n . vec2_n <= 1", typeof (V).Name)
                .Check (p => p.dot_vec1n_vec2n >= -1f && p.dot_vec1n_vec2n <= 1f);
            prop.Label ("{0}: vec1 . vec2 = (vec1 . vec2_n) * | vec2 |", typeof (V).Name)
                .Check (p => p.dot_vec1_vec2.ApproxEquals (p.dot_vec1_vec2n * p.len_vec2, 0.001f));
            prop.Label ("{0}: vec1 . vec2 = (vec2 . vec1_n) * | vec1 |", typeof (V).Name)
                .Check (p => p.dot_vec1_vec2.ApproxEquals (p.dot_vec2_vec1n * p.len_vec1, 0.001f));
        }

        [Test]
        public void TestAddSubtract ()
        {
            CheckAddSubtract<Vec2> ();
            CheckAddSubtract<Vec3> ();
            CheckAddSubtract<Vec4> ();
        }

        [Test]
        public void TestMultiply ()
        {
            CheckMultiplyWithScalar<Vec2> ();
            CheckMultiplyWithScalar<Vec3> ();
            CheckMultiplyWithScalar<Vec4> ();
            CheckMultiplyWithVector<Vec2> ();
            CheckMultiplyWithVector<Vec3> ();
            CheckMultiplyWithVector<Vec4> ();
        }

        [Test]
        public void TestDivide ()
        {
            CheckDivide<Vec2> ();
            CheckDivide<Vec3> ();
            CheckDivide<Vec4> ();
        }

        [Test]
        public void TestNormalize ()
        {
            CheckNormalize<Vec2> ();
            CheckNormalize<Vec3> ();
            CheckNormalize<Vec4> ();
        }

        [Test]
        public void TestDotProduct ()
        {
            CheckDotProduct<Vec2> ();
            CheckDotProduct<Vec3> ();
            CheckDotProduct<Vec4> ();
        }
    }
}

namespace ComposeTester
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Compose3D;
    using Compose3D.GLSL;
    using LinqCheck;

    public class VecTests
    {
        static VecTests ()
        {
            Arbitrary.Register (ArbitraryVec<Vec2, float> (2));
            Arbitrary.Register (ArbitraryVec<Vec3, float> (3));
            Arbitrary.Register (ArbitraryVec<Vec4, float> (4));
        }

        public static Arbitrary<V> ArbitraryVec<V, T> (int size) 
            where V : Vec<T>, new ()
            where T : struct, IEquatable<T>
        {
            var arb = Arbitrary.Get<T> (); 
            return new Arbitrary<V> ( 
                from a in arb.Generate.FixedArrayOf (size)
                select Vec<T>.Create<V> (a),
                v => from a in v.Vector.Combinations (arb.Shrink)
                     select Vec<T>.Create<V> (a));
        }

        public void CheckAddSubtract<V> () where V : Vec<float>, new ()
        {
            var prop = from vec1 in Prop.Choose<V> ()
                       from vec2 in Prop.Choose<V> ()
                       let neg = Vecf.Negate (vec2)
                       select new { vec1, vec2, neg };

            prop.Label ("{0}: vec1 - vec1 = [ 0 ... ]", typeof(V).Name)
                .Check (p => Vecf.Subtract (p.vec1, p.vec1) == new V ());
            prop.Label ("{0}: vec1 - vec2 = vec1 + (-vec2)", typeof (V).Name)
                .Check (p => Vecf.Subtract (p.vec1, p.vec2) == Vecf.Add (p.vec1, p.neg));
            prop.Label ("{0}: | vec1 + vec1 | = 2 * | vec1 |", typeof (V).Name)
                .Check (p => Vecf.Add (p.vec1, p.vec1).Length () == p.vec1.Length () * 2f);
        }

        public void CheckMultiplyWithScalar<V> () where V : Vec<float>, new ()
        {
            var prop = from vec in Prop.Choose<V> ()
                       from scalar in Prop.Choose<float> ()
                       let len = vec.Length ()
                       let scaled = Vecf.Multiply (vec, scalar)
                       let len_scaled = scaled.Length ()
                       let scalar_x_len = scalar * len
                       select new { vec, scalar, len, scaled, len_scaled, scalar_x_len };

            prop.Label ("{0}: | vec * scalar | = scalar * | vec |", typeof(V).Name)
                .Check (p => p.len_scaled.ApproxEquals (p.scalar_x_len));
        }

        public void CheckMultiplyWithVector<V> () where V : Vec<float>, new ()
        {
            var prop = from vec in Prop.Choose<V> ()
                       from scalar in Prop.Choose<float> ()
                       let len = vec.Length ()
                       let scaleVec = Vec<float>.Create<V> (scalar)
                       let scaled = Vecf.Multiply (vec, scaleVec)
                       let len_scaled = scaled.Length ()
                       let scalar_x_len = scaleVec[0] * len
                       select new { vec, scaleVec, len, scaled, len_scaled, scalar_x_len };

            prop.Label ("{0}: | vec * scale | = scale.x * | vec | when scale.xyzw are equal", typeof (V).Name)
                .Check (p => p.len_scaled.ApproxEquals (p.scalar_x_len));
        }

        public void CheckDivide<V> () where V : Vec<float>, new ()
        {
            var prop = from vec in Prop.Choose<V> ()
                       from scalar in Prop.Choose<float> ()
                       let divided = Vecf.Divide (vec, scalar)
                       let multiplied = Vecf.Multiply (vec, 1f / scalar)
                       select new { vec, scalar, divided, multiplied };

            prop.Label ("{0}: vec / scalar = vec * (1 / scalar)", typeof (V).Name)
                .Check (p => p.divided.ApproxEquals (p.multiplied));
        }

        public void CheckNormalize<V> () where V : Vec<float>, new ()
        {
            var prop = from vec in Prop.Choose<V> ()
                       let vec_n = vec.Normalize ()
                       let len = vec_n.Length ()
                       select new { vec, vec_n, len };

            prop.Label ("{0}: | vec_n | = 1", typeof (V).Name)
                .Check (p => p.len.ApproxEquals (1f));
        }

        public void CheckDotProduct<V> () where V : Vec<float>, new ()
        {
            var prop = from vec1 in Prop.Choose<V> ()
                       from vec2 in Prop.Choose<V> ()
                       let len_vec1 = vec1.Length ()
                       let len_vec2 = vec2.Length ()
                       let vec1n = vec1.Normalize ()
                       let vec2n = vec2.Normalize ()
                       let dot_vec1_vec2 = Vecf.Dot (vec1, vec2)
                       let dot_vec1n_vec2n = Vecf.Dot (vec1n, vec2n)
                       let dot_vec1_vec2n = Vecf.Dot (vec1, vec2n)
                       let dot_vec2_vec1n = Vecf.Dot (vec2, vec1n)
                       select new { vec1, vec2, len_vec1, len_vec2, vec1n, vec2n, 
                           dot_vec1_vec2, dot_vec1n_vec2n, dot_vec1_vec2n, dot_vec2_vec1n };

            prop.Label ("{0}: 0 <= vec1_n . vec2_n <= 1", typeof (V).Name)
                .Check (p => p.dot_vec1n_vec2n >= 0f && p.dot_vec1n_vec2n <= 1f);
            prop.Label ("{0}: vec1 . vec2 = (vec1 . vec2_n) * | vec2 |", typeof (V).Name)
                .Check (p => p.dot_vec1_vec2.ApproxEquals (p.dot_vec1_vec2n * p.len_vec2));
            prop.Label ("{0}: vec1 . vec2 = (vec2 . vec1_n) * | vec1 |", typeof (V).Name)
                .Check (p => p.dot_vec1_vec2.ApproxEquals (p.dot_vec2_vec1n * p.len_vec1));
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

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

        private static Arbitrary<V> ArbitraryVec<V, T> (int size) 
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
            var test = from vec1 in Prop.Choose<V> ()
                       from vec2 in Prop.Choose<V> ()
                       let neg = Vecf.Negate (vec2)
                       select new { vec1, vec2, neg };

            test.Label ("{0}: vec1 - vec1 = [ 0 ... ]", typeof(V).Name)
                .Check (t => Vecf.Subtract (t.vec1, t.vec1) == new V ());
            test.Label ("{0}: vec1 - vec2 = vec1 + (-vec2)", typeof (V).Name)
                .Check (t => Vecf.Subtract (t.vec1, t.vec2) == Vecf.Add (t.vec1, t.neg));
            test.Label ("{0}: | vec1 + vec1 | = 2 * | vec1 |", typeof (V).Name)
                .Check (t => Vecf.Add (t.vec1, t.vec1).Length () == t.vec1.Length () * 2f);
        }

        [Test]
        public void TestAddSubtract ()
        {
            CheckAddSubtract<Vec2> ();
            CheckAddSubtract<Vec3> ();
            CheckAddSubtract<Vec4> ();
        }
    }
}

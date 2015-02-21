using System;

namespace Compose3D.GLSL
{
    public static class Vecf 
    {
        public static V Add<V> (V left, V right) where V : Vec<float>, new ()
        {
            var res = new V ();
            left.Vector.MapWith (right.Vector, res.Vector, (a, b) => a + b);
            return res;
        }

        public static V Divide<V> (V vec, float scalar) where V : Vec<float>, new ()
        {
            var res = new V ();
            vec.Vector.Map (res.Vector, a => a / scalar);
            return res;
        }

        public static V Negate<V> (V vec) where V : Vec<float>, new ()
        {
            var res = new V ();
            vec.Vector.Map (res.Vector, a => -a);
            return res;
        }

        public static V Multiply<V> (V vec, float scalar) where V : Vec<float>, new ()
        {
            var res = new V ();
            vec.Vector.Map (res.Vector, a => a * scalar);
            return res;
        }

        public static V Multiply<V> (V vec, V scale) where V : Vec<float>, new ()
        {
            var res = new V ();
            vec.Vector.MapWith (scale.Vector, res.Vector, (a, b) => a * b);
            return res;
        }

        public static V Subtract<V> (V left, V right) where V : Vec<float>, new ()
        {
            var res = new V ();
            left.Vector.MapWith (right.Vector, res.Vector, (a, b) => a - b);
            return res;
        }

        public static float Dot<V> (V left, V right) where V : Vec<float>
        {
            return left.Vector.FoldWith (right.Vector, (res, a, b) => res + a * b, 0f);
        }

        public static float LengthSquared<V> (this V vec) where V : Vec<float>
        {
            return vec.Vector.Fold ((res, a) => res + a * a, 0f);
        }

        public static float Length<V> (this V vec) where V : Vec<float>
        {
            return (float)Math.Sqrt ((double)LengthSquared (vec));
        }

        public static V Normalize<V> (this V vec) where V : Vec<float>, new()
        {
            var len = Length (vec);
            var res = new V ();
            vec.Vector.Map (res.Vector, a => a / len);
            return res;
        }
    }
}

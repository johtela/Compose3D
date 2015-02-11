using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compose3D.GLSL
{
    public static class Vecf 
    {
        public static float Dot<V> (this V left, V right) where V : Vec<float>
        {
            return left.Vector.FoldWith (right.Vector, (res, a, b) => res += a * b, 0f);
        }

        public static float LengthSquared<V> (this V vec) where V : Vec<float>
        {
            return vec.Vector.Fold ((res, a) => res += a * a, 0f);
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

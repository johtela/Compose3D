namespace Compose3D.Arithmetics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public static class Vec
    {
        public static V FromArray<V, T> (params T[] items)
            where V : struct, IVec<V, T>
            where T : struct, IEquatable<T>
        {
            var res = default (V);
            for (int i = 0; i < Math.Min (res.Dimensions, items.Length); i++)
                res[i] = items[i];
            return res;
        }

        public static T[] ToArray<V, T> (this V vec)
            where V : struct, IVec<V, T>
            where T : struct, IEquatable<T>
        {
            var res = new T[vec.Dimensions];
            for (int i = 0; i < vec.Dimensions; i++)
                res[i] = vec[i];
            return res;
        }

        public static bool ApproxEquals<V> (V vec, V other)
            where V : struct, IVec<V, float>
        {
            for (int i = 0; i < vec.Dimensions; i++)
                if (!vec[i].ApproxEquals (other[i])) return false;
            return true;
        }

        public static V With<V, T> (this V vec, int i, T value)
            where V : struct, IVec<V, T>
            where T : struct, IEquatable<T>
        {
            var res = vec;
            res[i] = value;
            return res;
        }
    }
}

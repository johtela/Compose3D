using System;
using System.Linq;
using System.Text;

namespace Compose3D.GLSL
{
    public class Vec<T> : IEquatable<Vec<T>> where T : struct, IEquatable<T>
    {
        private T[] _vector;

        protected Vec (T[] vector)
        {
            _vector = vector;
        }

        public T[] Vector
        {
            get { return _vector; }
        }

        public T this[int index]
        {
            get { return _vector[index]; }
            set { _vector[index] = value; }
        }

        public bool Equals (Vec<T> other)
        {
            for (int i = 0; i < _vector.Length; i++)
                if (!_vector[i].Equals(other._vector[i])) return false;
            return true;
        }

        public override bool Equals (object obj)
        {
            var other = obj as Vec<T>;
            return !ReferenceEquals (other, null) && _vector.Equals (other._vector);
        }

        public override int GetHashCode ()
        {
            return _vector.GetHashCode ();
        }

        public override string ToString ()
        {
            var sb = new StringBuilder ("[");
            for (int i = 0; i < _vector.Length; i++)
                sb.AppendFormat (" {0}", _vector[i].ToString ());
            sb.Append (" ]");
            return sb.ToString ();
        }

        public static bool operator == (Vec<T> left, Vec<T> right)
        {
            return left.Equals (right);
        }

        public static bool operator != (Vec<T> left, Vec<T> right)
        {
            return !left.Equals (right);
        }

        public static V Create<V> (params T[] values) where V : Vec<T>, new ()
        {
            var res = new V ();
            var size = res.Vector.Length;
            var len = values.Length;
            for (int i = 0; i < size; i += len)
                Array.Copy (values, 0, res._vector, i, Math.Min (len, size - i));
            return res;
        }
    }

    public static class Vec
    {
        public static V With<V, T> (this V vec, int i, T value)
            where V : Vec<T>, new ()
            where T : struct, IEquatable<T>
        {
            var res = new V ();
            Array.Copy (vec.Vector, res.Vector, vec.Vector.Length);
            res[i] = value;
            return res;
        }
    }

}
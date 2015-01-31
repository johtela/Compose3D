using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compose3D.GLSL
{
    public abstract class Vec<TComp, TImpl> : IEquatable<Vec<TComp, TImpl>>, ICloneable
        where TImpl : IEquatable<TImpl>
    {
        protected internal TImpl _vector;

        protected abstract Vec<TComp, TImpl> Neg ();
        protected abstract Vec<TComp, TImpl> Add (Vec<TComp, TImpl> other);
        protected abstract Vec<TComp, TImpl> Sub (Vec<TComp, TImpl> other);
        protected abstract Vec<TComp, TImpl> Mul (TComp scalar);
        protected abstract Vec<TComp, TImpl> Mul (Vec<TComp, TImpl> other);
        protected abstract Vec<TComp, TImpl> Div (TComp scalar);
        public abstract TComp Dot (Vec<TComp, TImpl> other);
        public abstract TComp Length ();
        public abstract TComp Distance (Vec<TComp, TImpl> other);
        public abstract Vec<TComp, TImpl> Normalize ();
        public abstract object Clone ();

        public static Vec<TComp, TImpl> operator - (Vec<TComp, TImpl> vec)
        {
            return vec.Neg ();
        }

        public static Vec<TComp, TImpl> operator - (Vec<TComp, TImpl> left, Vec<TComp, TImpl> right)
        {
            return left.Sub (right);
        }

        public static bool operator != (Vec<TComp, TImpl> left, Vec<TComp, TImpl> right)
        {
            return !left.Equals (right);
        }

        public static Vec<TComp, TImpl> operator * (TComp scalar, Vec<TComp, TImpl> vec)
        {
            return vec.Mul (scalar);
        }

        public static Vec<TComp, TImpl> operator * (Vec<TComp, TImpl> vec, TComp scalar)
        {
            return vec.Mul (scalar);
        }

        public static Vec<TComp, TImpl> operator * (Vec<TComp, TImpl> vec, Vec<TComp, TImpl> scale)
        {
            return vec.Mul (scale);
        }

        public static Vec<TComp, TImpl> operator / (Vec<TComp, TImpl> vec, TComp scalar)
        {
            return vec.Div (scalar);
        }

        public static Vec<TComp, TImpl> operator + (Vec<TComp, TImpl> left, Vec<TComp, TImpl> right)
        {
            return left.Add (right);
        }

        public static bool operator == (Vec<TComp, TImpl> left, Vec<TComp, TImpl> right)
        {
            return left.Equals (right);
        }

        public bool Equals (Vec<TComp, TImpl> other)
        {
            return _vector.Equals (other._vector);
        }

        public override bool Equals (object obj)
        {
            return Equals ((Vec<TComp, TImpl>)obj);
        }

        public override int GetHashCode ()
        {
            return _vector.GetHashCode ();
        }
    }
}
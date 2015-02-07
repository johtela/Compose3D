using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Compose3D.GLSL
{
    public class Vec2
    {
        private Vector2 _vector;

        public Vec2 () { }

        public Vec2 (float x, float y)
        {
            _vector = new Vector2 (x, y);
        }

        private Vec2 (Vector2 vector)
        {
            _vector = vector;
        }

        public static Vec2 operator - (Vec2 vec)
        {
            return new Vec2 (-vec._vector);
        }

        public static Vec2 operator - (Vec2 left, Vec2 right)
        {
            var result = new Vec2 ();
            Vector2.Subtract (ref left._vector, ref right._vector, out result._vector);
            return result;
        }

        public static bool operator != (Vec2 left, Vec2 right)
        {
            return !left.Equals (right);
        }

        public static Vec2 operator * (float scalar, Vec2 vec)
        {
            var result = new Vec2 ();
            Vector2.Multiply (ref vec._vector, scalar, out result._vector);
            return result;
        }

        public static Vec2 operator * (Vec2 vec, float scalar)
        {
            return scalar * vec;
        }

        public static Vec2 operator * (Vec2 vec, Vec2 scale)
        {
            var result = new Vec2 ();
            Vector2.Multiply (ref vec._vector, ref scale._vector, out result._vector);
            return result;
        }

        public static Vec2 operator / (Vec2 vec, float scalar)
        {
            var result = new Vec2 ();
            Vector2.Divide (ref vec._vector, scalar, out result._vector);
            return result;
        }

        public static Vec2 operator + (Vec2 left, Vec2 right)
        {
            var result = new Vec2 ();
            Vector2.Add (ref left._vector, ref right._vector, out result._vector);
            return result;
        }

        public static bool operator == (Vec2 left, Vec2 right)
        {
            return left.Equals (right);
        }

        public bool Equals (Vec2 other)
        {
            return _vector.Equals (other._vector);
        }

        public override bool Equals (object obj)
        {
            return Equals ((Vec2)obj);
        }

        public override int GetHashCode ()
        {
            return _vector.GetHashCode ();
        }

        public float Dot (Vec2 other)
        {
            float result;
            Vector2.Dot (ref _vector, ref other._vector, out result);
            return result;
        }

        public float Length ()
        {
            return _vector.Length;
        }

        public float Distance (Vec2 other)
        {
            return (this - other).Length ();
        }

        public Vec2 Normalize ()
        {
            var result = new Vec2(_vector);
            result._vector.Normalize ();
            return result;
        }

        public float X 
        {
            get { return _vector.X; }
            set { _vector.X = value; }
        }

        public float Y
        {
            get { return _vector.Y; }
            set { _vector.Y = value; }
        }

        public float this[int index]
        {
            get { return _vector[index]; }
            set { _vector[index] = value; }
        }

        public Vec2 this[Coord x, Coord y]
        {
            get { return new Vec2 (this[(int)x], this[(int)y]); }
            set
            {
                this[(int)x] = value.X;
                this[(int)y] = value.Y;
            }
        }
    }
}
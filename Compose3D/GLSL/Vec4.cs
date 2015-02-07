using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Compose3D.GLSL
{
    public class Vec4
    {
        private Vector4 _vector;

        public Vec4 () { }

        public Vec4 (float x, float y, float z, float w)
        {
            _vector = new Vector4 (x, y, z, w);
        }

        private Vec4 (Vector4 vector)
        {
            _vector = vector;
        }

        public static Vec4 operator - (Vec4 vec)
        {
            return new Vec4 (-vec._vector);
        }

        public static Vec4 operator - (Vec4 left, Vec4 right)
        {
            var result = new Vec4 ();
            Vector4.Subtract (ref left._vector, ref right._vector, out result._vector);
            return result;
        }

        public static bool operator != (Vec4 left, Vec4 right)
        {
            return !left.Equals (right);
        }

        public static Vec4 operator * (float scalar, Vec4 vec)
        {
            var result = new Vec4 ();
            Vector4.Multiply (ref vec._vector, scalar, out result._vector);
            return result;
        }

        public static Vec4 operator * (Vec4 vec, float scalar)
        {
            return scalar * vec;
        }

        public static Vec4 operator * (Vec4 vec, Vec4 scale)
        {
            var result = new Vec4 ();
            Vector4.Multiply (ref vec._vector, ref scale._vector, out result._vector);
            return result;
        }

        public static Vec4 operator / (Vec4 vec, float scalar)
        {
            var result = new Vec4 ();
            Vector4.Divide (ref vec._vector, scalar, out result._vector);
            return result;
        }

        public static Vec4 operator + (Vec4 left, Vec4 right)
        {
            var result = new Vec4 ();
            Vector4.Add (ref left._vector, ref right._vector, out result._vector);
            return result;
        }

        public static bool operator == (Vec4 left, Vec4 right)
        {
            return left.Equals (right);
        }

        public bool Equals (Vec4 other)
        {
            return _vector.Equals (other._vector);
        }

        public override bool Equals (object obj)
        {
            return Equals ((Vec4)obj);
        }

        public override int GetHashCode ()
        {
            return _vector.GetHashCode ();
        }

        public float Dot (Vec4 other)
        {
            float result;
            Vector4.Dot (ref _vector, ref other._vector, out result);
            return result;
        }

        public float Length ()
        {
            return _vector.Length;
        }

        public float Distance (Vec4 other)
        {
            return (this - other).Length ();
        }

        public Vec4 Normalize ()
        {
            var result = new Vec4(_vector);
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

        public float Z
        {
            get { return _vector.Z; }
            set { _vector.Z = value; }
        }

        public float W
        {
            get { return _vector.W; }
            set { _vector.W = value; }
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

        public Vec3 this[Coord x, Coord y, Coord z]
        {
            get { return new Vec3 (this[(int)x], this[(int)y], this[(int)z]); }
            set
            {
                this[(int)x] = value.X;
                this[(int)y] = value.Y;
                this[(int)z] = value.Z;
            }
        }

        public Vec4 this[Coord x, Coord y, Coord z, Coord w]
        {
            get { return new Vec4 (this[(int)x], this[(int)y], this[(int)z], this[(int)w]); }
            set
            {
                this[(int)x] = value.X;
                this[(int)y] = value.Y;
                this[(int)z] = value.Z;
                this[(int)w] = value.W;
            }
        }
    }
}
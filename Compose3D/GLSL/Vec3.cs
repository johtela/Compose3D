using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Compose3D.GLSL
{
    public class Vec3
    {
        private Vector3 _vector;

        public Vec3 () { }

        public Vec3 (float x, float y, float z)
        {
            _vector = new Vector3 (x, y, z);
        }

        private Vec3 (Vector3 vector)
        {
            _vector = vector;
        }

        public static Vec3 operator - (Vec3 vec)
        {
            return new Vec3 (-vec._vector);
        }

        public static Vec3 operator - (Vec3 left, Vec3 right)
        {
            var result = new Vec3 ();
            Vector3.Subtract (ref left._vector, ref right._vector, out result._vector);
            return result;
        }

        public static bool operator != (Vec3 left, Vec3 right)
        {
            return !left.Equals (right);
        }

        public static Vec3 operator * (float scalar, Vec3 vec)
        {
            var result = new Vec3 ();
            Vector3.Multiply (ref vec._vector, scalar, out result._vector);
            return result;
        }

        public static Vec3 operator * (Vec3 vec, float scalar)
        {
            return scalar * vec;
        }

        public static Vec3 operator * (Vec3 vec, Vec3 scale)
        {
            var result = new Vec3 ();
            Vector3.Multiply (ref vec._vector, ref scale._vector, out result._vector);
            return result;
        }

        public static Vec3 operator / (Vec3 vec, float scalar)
        {
            var result = new Vec3 ();
            Vector3.Divide (ref vec._vector, scalar, out result._vector);
            return result;
        }

        public static Vec3 operator + (Vec3 left, Vec3 right)
        {
            var result = new Vec3 ();
            Vector3.Add (ref left._vector, ref right._vector, out result._vector);
            return result;
        }

        public static bool operator == (Vec3 left, Vec3 right)
        {
            return left.Equals (right);
        }

        public bool Equals (Vec3 other)
        {
            return _vector.Equals (other._vector);
        }

        public override bool Equals (object obj)
        {
            return Equals ((Vec3)obj);
        }

        public override int GetHashCode ()
        {
            return _vector.GetHashCode ();
        }

        public float Dot (Vec3 other)
        {
            float result;
            Vector3.Dot (ref _vector, ref other._vector, out result);
            return result;
        }

        public float Length ()
        {
            return _vector.Length;
        }

        public float Distance (Vec3 other)
        {
            return (this - other).Length ();
        }

        public Vec3 Normalize ()
        {
            var result = new Vec3(_vector);
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
    }
}
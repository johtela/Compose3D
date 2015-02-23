namespace Compose3D.GLSL
{
    using System;
    using OpenTK;

    public class Vec2 : Vec<float>
    {
        public Vec2 () : base (new float[2]) { }

        public Vec2 (float x, float y) : base (new float[] { x, y }) { }

        public Vec2 (Vec<float> vec) : this (vec[0], vec[1]) { }

        internal Vec2 (float[] vector) : base (vector) { }

        public static Vec2 operator - (Vec2 vec)
        {
            return Vecf.Negate (vec);
        }

        public static Vec2 operator - (Vec2 left, Vec2 right)
        {
            return Vecf.Subtract (left, right);
        }

        public static Vec2 operator * (float scalar, Vec2 vec)
        {
            return Vecf.MultiplyScalar (vec, scalar);
        }

        public static Vec2 operator * (Vec2 vec, float scalar)
        {
            return Vecf.MultiplyScalar (vec, scalar);
        }

        public static Vec2 operator * (Vec2 vec, Vec2 scale)
        {
            return Vecf.Multiply (vec, scale);
        }

        public static Vec2 operator / (Vec2 vec, float scalar)
        {
            return Vecf.Divide (vec, scalar);
        }

        public static Vec2 operator + (Vec2 left, Vec2 right)
        {
            return Vecf.Add (left, right);
        }

        public static implicit operator Vector2 (Vec2 vec)
        {
            return new Vector2 (vec.X, vec.Y);
        }

        public static implicit operator Vec2 (Vector2 vec)
        {
            return new Vec2 (vec.X, vec.Y);
        }

        public float X 
        {
            get { return Vector[0]; }
            set { Vector[0] = value; }
        }

        public float Y
        {
            get { return Vector[1]; }
            set { Vector[1] = value; }
        }

        public Vec2 this[Coord x, Coord y]
        {
            get { return new Vec2 (Vector[(int)x], Vector[(int)y]); }
            set
            {
                Vector[(int)x] = value[0];
                Vector[(int)y] = value[1];
            }
        }
    }
}
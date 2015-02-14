using System;

namespace Compose3D.GLSL
{
    public class Vec4 : Vec<float>
    {
        public Vec4 () : base (new float[4]) { }

        public Vec4 (float x, float y, float z, float w) : base (new float[] { x, y, z, w }) { }

        internal Vec4 (float[] vector) : base (vector) { }

        public static Vec4 operator - (Vec4 vec)
        {
            return new Vec4 (vec.Vector.Map (a => -a));
        }

        public static Vec4 operator - (Vec4 left, Vec4 right)
        {
            return new Vec4 (left.Vector.MapWith (right.Vector, (a, b) => a - b));
        }

        public static Vec4 operator * (float scalar, Vec4 vec)
        {
            return new Vec4 (vec.Vector.Map (a => a * scalar));
        }

        public static Vec4 operator * (Vec4 vec, float scalar)
        {
            return scalar * vec;
        }

        public static Vec4 operator * (Vec4 vec, Vec4 scale)
        {
            return new Vec4 (vec.Vector.MapWith (scale.Vector, (a, b) => a * b));
        }

        public static Vec4 operator / (Vec4 vec, float scalar)
        {
            return new Vec4 (vec.Vector.Map (a => a / scalar));
        }

        public static Vec4 operator + (Vec4 left, Vec4 right)
        {
            return new Vec4 (left.Vector.MapWith (right.Vector, (a, b) => a + b));
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

        public float Z
        {
            get { return Vector[2]; }
            set { Vector[2] = value; }
        }

        public float W
        {
            get { return Vector[3]; }
            set { Vector[3] = value; }
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

        public Vec3 this[Coord x, Coord y, Coord z]
        {
            get { return new Vec3 (Vector[(int)x], Vector[(int)y], Vector[(int)z]); }
            set
            {
                Vector[(int)x] = value[0];
                Vector[(int)y] = value[1];
                Vector[(int)z] = value[2];
            }
        }

        public Vec4 this[Coord x, Coord y, Coord z, Coord w]
        {
            get { return new Vec4 (Vector[(int)x], Vector[(int)y], Vector[(int)z], Vector[(int)w]); }
            set
            {
                Vector[(int)x] = value[0];
                Vector[(int)y] = value[1];
                Vector[(int)z] = value[2];
                Vector[(int)w] = value[2];
            }
        }
    }
}
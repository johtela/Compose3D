using System;

namespace Compose3D.GLSL
{
    public class Vec3 : Vec<float>
    {
        public Vec3 () : base (new float[3]) { }

        public Vec3 (float x, float y, float z) : base (new float[] { x, y, z }) { }

        protected Vec3 (float[] vector) : base (vector) { }

        public static Vec3 operator - (Vec3 vec)
        {
            return new Vec3 (vec.Vector.Map (a => -a));
        }

        public static Vec3 operator - (Vec3 left, Vec3 right)
        {
            return new Vec3 (left.Vector.MapWith (right.Vector, (a, b) => a - b));
        }

        public static Vec3 operator * (float scalar, Vec3 vec)
        {
            return new Vec3 (vec.Vector.Map (a => a * scalar));
        }

        public static Vec3 operator * (Vec3 vec, float scalar)
        {
            return scalar * vec;
        }

        public static Vec3 operator * (Vec3 vec, Vec3 scale)
        {
            return new Vec3 (vec.Vector.MapWith (scale.Vector, (a, b) => a * b));
        }

        public static Vec3 operator / (Vec3 vec, float scalar)
        {
            return new Vec3 (vec.Vector.Map (a => a / scalar));
        }

        public static Vec3 operator + (Vec3 left, Vec3 right)
        {
            return new Vec3 (left.Vector.MapWith (right.Vector, (a, b) => a + b));
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
    }
}
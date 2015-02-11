using System;

namespace Compose3D.GLSL
{
    public class Vec2 : Vec<float>
    {
        public Vec2 () : base (new float[2]) { }

        public Vec2 (float x, float y) : base (new float[] { x, y }) { }

        internal Vec2 (float[] vector) : base (vector) { }

        public static Vec2 operator - (Vec2 vec)
        {
            return new Vec2 (vec.Vector.Map (a => -a));
        }

        public static Vec2 operator - (Vec2 left, Vec2 right)
        {
            return new Vec2 (left.Vector.MapWith (right.Vector, (a, b) => a - b));
        }

        public static Vec2 operator * (float scalar, Vec2 vec)
        {
            return new Vec2 (vec.Vector.Map (a => a * scalar));
        }

        public static Vec2 operator * (Vec2 vec, float scalar)
        {
            return scalar * vec;
        }

        public static Vec2 operator * (Vec2 vec, Vec2 scale)
        {
            return new Vec2 (vec.Vector.MapWith (scale.Vector, (a, b) => a * b));
        }

        public static Vec2 operator / (Vec2 vec, float scalar)
        {
            return new Vec2 (vec.Vector.Map (a => a / scalar));
        }

        public static Vec2 operator + (Vec2 left, Vec2 right)
        {
            return new Vec2 (left.Vector.MapWith (right.Vector, (a, b) => a + b));
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
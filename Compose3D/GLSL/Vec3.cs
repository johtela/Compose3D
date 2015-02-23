namespace Compose3D.GLSL
{
    using System;
    using OpenTK;

    public class Vec3 : Vec<float>
    {
        public Vec3 () : base (new float[3]) { }

        public Vec3 (float x, float y, float z) : base (new float[] { x, y, z }) { }

        public Vec3 (Vec2 vec, float z) : this (vec.X, vec.Y, z) { }

        public Vec3 (Vec<float> vec) : this (vec[0], vec[1], vec[2]) { }

        internal Vec3 (float[] vector) : base (vector) { }

        public static Vec3 operator - (Vec3 vec)
        {
            return Vecf.Negate (vec);
        }

        public static Vec3 operator - (Vec3 left, Vec3 right)
        {
            return Vecf.Subtract (left, right);
        }

        public static Vec3 operator * (float scalar, Vec3 vec)
        {
            return Vecf.MultiplyScalar (vec, scalar);
        }

        public static Vec3 operator * (Vec3 vec, float scalar)
        {
            return Vecf.MultiplyScalar (vec, scalar);
        }

        public static Vec3 operator * (Vec3 vec, Vec3 scale)
        {
            return Vecf.Multiply (vec, scale);
        }

        public static Vec3 operator / (Vec3 vec, float scalar)
        {
            return Vecf.Divide (vec, scalar);
        }

        public static Vec3 operator + (Vec3 left, Vec3 right)
        {
            return Vecf.Add (left, right);
        }

        public static implicit operator Vector3 (Vec3 vec)
        {
            return new Vector3 (vec.X, vec.Y, vec.Z);
        }

        public static implicit operator Vec3 (Vector3 vec)
        {
            return new Vec3 (vec.X, vec.Y, vec.Z);
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
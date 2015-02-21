namespace Compose3D.GLSL
{
    public class Mat3 : Mat<float>
    {
        public Mat3 () : base (new float[3, 3]) { }

        public Mat3 (float m11, float m12, float m13, 
            float m21, float m22, float m23,
            float m31, float m32, float m33) :
            base (new float[,] { { m11, m12, m13 }, { m21, m22, m23 }, { m31, m32, m33 } }) { }

        protected Mat3 (float[,] matrix) : base (matrix) { }

        public Mat3 (Vec3 col1, Vec3 col2, Vec3 col3) :
            this (col1[0], col1[1], col1[2], col2[0], col2[1], col2[2], col3[0], col3[1], col3[2]) { }

        public static Mat3 operator - (Mat3 left, Mat3 right)
        {
            return Matf.Subtract (left, right);
        }

        public static Mat3 operator * (float scalar, Mat3 mat)
        {
            return Matf.Multiply (mat, scalar);
        }

        public static Mat3 operator * (Mat3 mat, float scalar)
        {
            return Matf.Multiply (mat, scalar);
        }

        public static Mat3 operator * (Mat3 left, Mat3 right)
        {
            return Matf.Multiply<Mat3> (left, right);
        }

        public static Vec3 operator * (Mat3 mat, Vec3 vec)
        {
            return Matf.Multiply (mat, vec);
        }

        public static Mat3 operator + (Mat3 left, Mat3 right)
        {
            return Matf.Add (left, right);
        }

        public Vec3 this[int col]
        {
            get { return new Vec3 (Matrix.GetColumn (col)); }
            set { Matrix.SetColumn (col, value.Vector); }
        }
    }
}
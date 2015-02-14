namespace Compose3D.GLSL
{
    public class Mat4 : Mat<float>
    {
        public Mat4 () : base (new float[4, 4]) { }

        public Mat4 (float m11, float m12, float m13, float m14,
            float m21, float m22, float m23, float m24,
            float m31, float m32, float m33, float m34,
            float m41, float m42, float m43, float m44) :
            base (new float[,] { 
                { m11, m12, m13, m14 }, 
                { m21, m22, m23, m24 }, 
                { m31, m32, m33, m34 }, 
                { m41, m42, m43, m44 } }) { }

        protected Mat4 (float[,] matrix) : base (matrix) { }

        public Mat4 (Vec4 col1, Vec4 col2, Vec4 col3, Vec4 col4) :
            this (col1[0], col1[1], col1[2], col1[3], 
                col2[0], col2[1], col2[2], col2[3],
                col3[0], col3[1], col3[2], col3[3],
                col4[0], col4[1], col4[2], col4[3]) { }

        public static Mat4 operator - (Mat4 left, Mat4 right)
        {
            return left.MapWith<Mat4, float> (right, (a, b) => a - b);
        }

        public static Mat4 operator * (float scalar, Mat4 mat)
        {
            return mat.Map<Mat4, float> (a => a * scalar);
        }

        public static Mat4 operator * (Mat4 vec, float scalar)
        {
            return scalar * vec;
        }

        public static Mat4 operator * (Mat4 left, Mat4 right)
        {
            return left.Multiply<Mat4, float> (right, (s, a, b) => s + a * b);
        }

        public static Vec4 operator * (Mat4 mat, Vec4 vec)
        {
            var result = new Vec4 ();
            mat.Matrix.Multiply (vec.Vector, result.Vector, (s, a, b) => s + a * b);
            return result;
        }

        public static Mat4 operator + (Mat4 left, Mat4 right)
        {
            return left.MapWith<Mat4, float> (right, (a, b) => a + b);
        }

        public Vec4 this[int col]
        {
            get { return new Vec4 (Matrix.GetColumn (col)); }
            set { Matrix.SetColumn (col, value.Vector); }
        }
    }
}
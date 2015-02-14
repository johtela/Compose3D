namespace Compose3D.GLSL
{
    public class Mat2 : Mat<float>
    {
        public Mat2 () : base (new float[2, 2]) { }

        public Mat2 (float m11, float m12, float m21, float m22) : 
            base (new float[,] { { m11, m12 }, { m21, m22 } }) { }

        protected Mat2 (float[,] matrix) : base (matrix) { }

        public Mat2 (Vec2 col1, Vec2 col2) :
            this (col1[0], col1[1], col2[0], col2[1]) { }

        public static Mat2 operator - (Mat2 left, Mat2 right)
        {
            return left.MapWith<Mat2, float> (right, (a, b) => a - b);
        }

        public static Mat2 operator * (float scalar, Mat2 mat)
        {
            return mat.Map<Mat2, float> (a => a * scalar);
        }

        public static Mat2 operator * (Mat2 vec, float scalar)
        {
            return scalar * vec;
        }

        public static Mat2 operator * (Mat2 left, Mat2 right)
        {
            return left.Multiply<Mat2, float> (right, (s, a, b) => s + a * b);
        }

        public static Vec2 operator * (Mat2 mat, Vec2 vec)
        {
            var result = new Vec2 ();
            mat.Matrix.Multiply (vec.Vector, result.Vector, (s, a, b) => s + a * b);
            return result;
        }

        public static Mat2 operator + (Mat2 left, Mat2 right)
        {
            return left.MapWith<Mat2, float> (right, (a, b) => a + b);
        }

        public Vec2 this[int col]
        {
            get { return new Vec2 (Matrix.GetColumn (col)); }
            set { Matrix.SetColumn (col, value.Vector); }
        }
    }
}
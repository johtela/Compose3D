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

        public static Mat2 operator - (Mat2 mat)
        {
            return Matf.Negate (mat);
        }

        public static Mat2 operator - (Mat2 left, Mat2 right)
        {
            return Matf.Subtract (left, right);
        }

        public static Mat2 operator * (float scalar, Mat2 mat)
        {
            return Matf.MultiplyScalar (mat, scalar);
        }

        public static Mat2 operator * (Mat2 mat, float scalar)
        {
            return Matf.MultiplyScalar (mat, scalar);
        }

        public static Mat2 operator * (Mat2 left, Mat2 right)
        {
            return Matf.Multiply<Mat2> (left, right);
        }

        public static Vec2 operator * (Mat2 mat, Vec2 vec)
        {
            return Matf.MultiplyVector (mat, vec);
        }

        public static Mat2 operator + (Mat2 left, Mat2 right)
        {
            return Matf.Add (left, right);
        }

        public Vec2 this[int col]
        {
            get { return new Vec2 (Matrix.GetColumn (col)); }
            set { Matrix.SetColumn (col, value.Vector); }
        }
    }
}
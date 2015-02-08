using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Compose3D.GLSL
{
    public class Mat2
    {
        private Matrix2 _matrix;

        public Mat2 () { }

        public Mat2 (float m11, float m12, float m21, float m22)
        {
            _matrix = new Matrix2 (m11, m21, m12, m22);
        }

        public Mat2 (Vec2 col1, Vec2 col2)
        {
            _matrix = new Matrix2 (col1.X, col2.X, col1.Y, col2.Y);
        }

        private Mat2 (Matrix2 matrix)
        {
            _matrix = matrix;
        }

        public static Mat2 operator - (Mat2 left, Mat2 right)
        {
            var result = new Mat2 ();
            Matrix2.Subtract (ref left._matrix, ref right._matrix, out result._matrix);
            return result;
        }

        public static bool operator != (Mat2 left, Mat2 right)
        {
            return !left.Equals (right);
        }

        public static Mat2 operator * (float scalar, Mat2 vec)
        {
            var result = new Mat2 ();
            Matrix2.Mult (ref vec._matrix, scalar, out result._matrix);
            return result;
        }

        public static Mat2 operator * (Mat2 vec, float scalar)
        {
            return scalar * vec;
        }

        public static Mat2 operator * (Mat2 left, Mat2 right)
        {
            var result = new Mat2 ();
            Matrix2.Mult (ref left._matrix, ref right._matrix, out result._matrix);
            return result;
        }

        public static Vec2 operator * (Mat2 mat, Vec2 vec)
        {
            var result = new Vec2 ();
            result.X = mat._matrix.M11 * vec.X + mat._matrix.M12 * vec.Y;
            result.Y = mat._matrix.M21 * vec.X + mat._matrix.M22 * vec.Y;
            return result;
        }

        public static Mat2 operator + (Mat2 left, Mat2 right)
        {
            var result = new Mat2 ();
            Matrix2.Add (ref left._matrix, ref right._matrix, out result._matrix);
            return result;
        }

        public static bool operator == (Mat2 left, Mat2 right)
        {
            return left.Equals (right);
        }

        public bool Equals (Mat2 other)
        {
            return _matrix.Equals (other._matrix);
        }

        public override bool Equals (object obj)
        {
            return Equals ((Mat2)obj);
        }

        public override int GetHashCode ()
        {
            return _matrix.GetHashCode ();
        }

        public Vec2 this[int col]
        {
            get
            {
                switch (col)
                {
                    case 0: return new Vec2 (_matrix.Column0);
                    case 1: return new Vec2 (_matrix.Column1);
                    default: throw new IndexOutOfRangeException ("Column index out of bounds: " + col);
                }
            }
            set
            {
                switch (col)
                {
                    case 0: 
                        _matrix.Column0 = value._vector;
                        break;
                    case 1:
                        _matrix.Column1 = value._vector;
                        break;
                    default: 
                        throw new IndexOutOfRangeException ("Column index out of bounds: " + col);
                }
            }
        }

        public float this[int col, int row] 
        {
            get { return _matrix[row, col]; }
            set { _matrix[row, col] = value; }
        }
    }
}

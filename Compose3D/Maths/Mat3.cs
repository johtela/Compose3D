﻿namespace Compose3D.Maths
{
    using System;
    using System.Text;
	using GLTypes;

	[GLType ("mat3")]
    public struct Mat3 : ISquareMat<Mat3, float>
    { 
		public Vec3 Column0; 
		public Vec3 Column1; 
		public Vec3 Column2; 

		[GLConstructor ("mat3 ({0})")]
		public Mat3 (Vec3 column0, Vec3 column1, Vec3 column2)
		{
			Column0 = column0; 
			Column1 = column1; 
			Column2 = column2; 
		}

 		[GLConstructor ("mat3 ({0})")]
		public Mat3 (Mat2 mat)
		{	
			Column0 = new Vec3 (mat.Column0, 0);
			Column1 = new Vec3 (mat.Column1, 0);
			Column2 = new Vec3 (0, 0, 1);
		}

		[GLConstructor ("mat3 ({0})")]
		public Mat3 (Mat3 mat)
		{	
			Column0 = new Vec3 (mat.Column0);
			Column1 = new Vec3 (mat.Column1);
			Column2 = new Vec3 (mat.Column2);
		}

		[GLConstructor ("mat3 ({0})")]
		public Mat3 (Mat4 mat)
		{	
			Column0 = new Vec3 (mat.Column0);
			Column1 = new Vec3 (mat.Column1);
			Column2 = new Vec3 (mat.Column2);
		}

		public Mat3 (float value)
		{	
			Column0 = new Vec3 (value, 0, 0); 
			Column1 = new Vec3 (0, value, 0); 
			Column2 = new Vec3 (0, 0, value); 
		}

 		[GLConstructor ("mat3 ({0})")]
		public Mat3 (
			float m00, float m01, float m02, 
			float m10, float m11, float m12, 
			float m20, float m21, float m22)
		{	
			Column0 = new Vec3 (m00, m01, m02); 
			Column1 = new Vec3 (m10, m11, m12); 
			Column2 = new Vec3 (m20, m21, m22); 
		}

 		public int Columns
		{
			get { return 3; }
		}

		public int Rows
		{
			get { return 3; }
		}

		public Vec3 this[int column]
		{
			get
			{
				switch (column)
				{	         
					case 0: return Column0;          
					case 1: return Column1;          
					case 2: return Column2; 
			        default: throw new ArgumentOutOfRangeException("column");
				}
			} 
			set
			{
				switch (column)
				{	         
					case 0: Column0 = value; break;          
					case 1: Column1 = value; break;          
					case 2: Column2 = value; break; 
			        default: throw new ArgumentOutOfRangeException("column");
				}
			} 
		}

		public float this[int column, int row]
		{
			get { return this[column][row]; }
			set 
            { 
				switch (column)
				{	         
					case 0: Column0[row] = value; break;          
					case 1: Column1[row] = value; break;          
					case 2: Column2[row] = value; break; 
			        default: throw new ArgumentOutOfRangeException("column");
				}
            }
		} 
					
		[GLBinaryOperator ("{0} + {1}")]
		public Mat3 Add (Mat3 other)
		{
			return new Mat3 (Column0 + other.Column0, Column1 + other.Column1, Column2 + other.Column2);
		}

		[GLBinaryOperator ("{0} - {1}")]
		public Mat3 Subtract (Mat3 other)
		{
			return new Mat3 (Column0 - other.Column0, Column1 - other.Column1, Column2 - other.Column2);
		}

		[GLBinaryOperator ("{0} * {1}")]
		public Mat3 Multiply (float scalar)
		{
			return new Mat3 (Column0 * scalar, Column1 * scalar, Column2 * scalar);
		}

		[GLBinaryOperator ("{0} / {1}")]
		public Mat3 Divide (float scalar)
		{
			return new Mat3 (Column0 / scalar, Column1 / scalar, Column2 / scalar);
		}

        public V Multiply<V> (V vec) where V : struct, IVec<V, float>, IEquatable<V>
        {
            if (vec.Dimensions != Columns)
                throw new ArgumentException (
					string.Format ("Cannot multiply {0}x{1} matrix with {2}D vector", Columns, Rows, vec.Dimensions), "vec");
            var res = default (V); 
			res[0] = Column0.X * vec[0] + Column1.X * vec[1] + Column2.X * vec[2];
			res[1] = Column0.Y * vec[0] + Column1.Y * vec[1] + Column2.Y * vec[2];
			res[2] = Column0.Z * vec[0] + Column1.Z * vec[1] + Column2.Z * vec[2];
			return res;
        }

		public bool Equals (Mat3 other)
		{
			return Column0 == other.Column0 && Column1 == other.Column1 && Column2 == other.Column2;
		}

		[GLBinaryOperator ("{0} * {1}")]
        public Mat3 Multiply (Mat3 mat)
        {
            return this * mat;
        }

		[GLFunction ("transpose ({0})")]
        public Mat3 Transposed
        {
            get { return Mat.Transpose<Mat3, float> (this); }
        }

		[GLFunction ("determinant ({0})")]
        public float Determinant
        {
            get { return Mat.Determinant (this); }
        }

		[GLFunction ("inverse ({0})")]
        public Mat3 Inverse
        {
            get { return Mat.Inverse (this); }
        }

		public override bool Equals (object obj)
		{
            return obj is Mat3 && Equals ((Mat3)obj);
		}

        public override int GetHashCode ()
        {
			return Column0.GetHashCode () ^ Column1.GetHashCode () ^ Column2.GetHashCode ();
        }

        public override string ToString ()
        {
            var sb = new StringBuilder ();
            sb.AppendLine ();
            for (int r = 0; r < 3; r++)
            {
                sb.Append ("[");
                for (int c = 0; c < 3; c++)
                    sb.AppendFormat (" {0}", this[c, r]);
                sb.AppendLine (" ]");
            }
            return sb.ToString ();
        }

		[GLBinaryOperator ("{0} - {1}")]
       public static Mat3 operator - (Mat3 left, Mat3 right)
        {
            return left.Subtract (right);
        }

		[GLBinaryOperator ("{0} * {1}")]
        public static Mat3 operator * (float scalar, Mat3 mat)
        {
            return mat.Multiply (scalar);
        }

		[GLBinaryOperator ("{0} * {1}")]
        public static Mat3 operator * (Mat3 mat, float scalar)
        {
            return mat.Multiply (scalar);
        }

		[GLBinaryOperator ("{0} * {1}")]
        public static Mat3 operator * (Mat3 left, Mat3 right)
        {
			return new Mat3 (
				left.Column0.X * right.Column0.X + left.Column1.X * right.Column0.Y + left.Column2.X * right.Column0.Z,
				left.Column0.Y * right.Column0.X + left.Column1.Y * right.Column0.Y + left.Column2.Y * right.Column0.Z,
				left.Column0.Z * right.Column0.X + left.Column1.Z * right.Column0.Y + left.Column2.Z * right.Column0.Z,
				left.Column0.X * right.Column1.X + left.Column1.X * right.Column1.Y + left.Column2.X * right.Column1.Z,
				left.Column0.Y * right.Column1.X + left.Column1.Y * right.Column1.Y + left.Column2.Y * right.Column1.Z,
				left.Column0.Z * right.Column1.X + left.Column1.Z * right.Column1.Y + left.Column2.Z * right.Column1.Z,
				left.Column0.X * right.Column2.X + left.Column1.X * right.Column2.Y + left.Column2.X * right.Column2.Z,
				left.Column0.Y * right.Column2.X + left.Column1.Y * right.Column2.Y + left.Column2.Y * right.Column2.Z,
				left.Column0.Z * right.Column2.X + left.Column1.Z * right.Column2.Y + left.Column2.Z * right.Column2.Z);
        }

		[GLBinaryOperator ("{0} * {1}")]
        public static Vec3 operator * (Mat3 mat, Vec3 vec)
        {
			return new Vec3 (
				mat.Column0.X * vec.X + mat.Column1.X * vec.Y + mat.Column2.X * vec.Z,
				mat.Column0.Y * vec.X + mat.Column1.Y * vec.Y + mat.Column2.Y * vec.Z,
				mat.Column0.Z * vec.X + mat.Column1.Z * vec.Y + mat.Column2.Z * vec.Z);
        }

		[GLBinaryOperator ("{0} / {1}")]
        public static Mat3 operator / (Mat3 mat, float scalar)
        {
            return mat.Divide (scalar);
        }

		[GLBinaryOperator ("{0} + {1}")]
        public static Mat3 operator + (Mat3 left, Mat3 right)
        {
            return left.Add (right);
        }

        public static bool operator == (Mat3 left, Mat3 right)
        {
            return left.Equals (right);
        }

        public static bool operator != (Mat3 left, Mat3 right)
        {
            return !left.Equals (right);
        }
	}
}
 
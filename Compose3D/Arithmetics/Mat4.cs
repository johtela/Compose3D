﻿namespace Compose3D.Arithmetics
{
    using System;
    using System.Text;

    public struct Mat4 : IMat<Mat4, float>, IEquatable<Mat4>
    { 
		public Vec4 Column0; 
		public Vec4 Column1; 
		public Vec4 Column2; 
		public Vec4 Column3; 

		public Mat4 (Vec4 column0, Vec4 column1, Vec4 column2, Vec4 column3)
		{
			Column0 = column0; 
			Column1 = column1; 
			Column2 = column2; 
			Column3 = column3; 
		}

 		public Mat4 (float value)
		{	
			Column0 = new Vec4 (value, 0, 0, 0); 
			Column1 = new Vec4 (0, value, 0, 0); 
			Column2 = new Vec4 (0, 0, value, 0); 
			Column3 = new Vec4 (0, 0, 0, value); 
		}

 		public Mat4 (
			float m00, float m01, float m02, float m03, 
			float m10, float m11, float m12, float m13, 
			float m20, float m21, float m22, float m23, 
			float m30, float m31, float m32, float m33)
		{	
			Column0 = new Vec4 (m00, m01, m02, m03); 
			Column1 = new Vec4 (m10, m11, m12, m13); 
			Column2 = new Vec4 (m20, m21, m22, m23); 
			Column3 = new Vec4 (m30, m31, m32, m33); 
		}

 		public int Columns
		{
			get { return 4; }
		}

		public int Rows
		{
			get { return 4; }
		}

		public Vec4 this[int column]
		{
			get
			{
				switch (column)
				{	         
					case 0: return Column0;          
					case 1: return Column1;          
					case 2: return Column2;          
					case 3: return Column3; 
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
					case 3: Column3 = value; break; 
			        default: throw new ArgumentOutOfRangeException("column");
				}
			} 
		}

		public float this[int column, int row]
		{
			get { return this[column][row]; }
			set 
            { 
                var vec = this[column];
                vec[row] = value; 
            }
		} 
					
		public Mat4 Add (Mat4 other)
		{
			return new Mat4 (Column0 + other.Column0, Column1 + other.Column1, Column2 + other.Column2, Column3 + other.Column3);
		}

		public Mat4 Subtract (Mat4 other)
		{
			return new Mat4 (Column0 - other.Column0, Column1 - other.Column1, Column2 - other.Column2, Column3 - other.Column3);
		}

		public Mat4 Multiply (float scalar)
		{
			return new Mat4 (Column0 * scalar, Column1 * scalar, Column2 * scalar, Column3 * scalar);
		}

		public Mat4 Divide (float scalar)
		{
			return new Mat4 (Column0 / scalar, Column1 / scalar, Column2 / scalar, Column3 / scalar);
		}

        public V Multiply<V> (V vec) where V : struct, IVec<V, float>, IEquatable<V>
        {
            if (vec.Dimensions != Columns)
                throw new ArgumentException (
					string.Format ("Cannot multiply {0}x{1} matrix with {2}D vector", Columns, Rows, vec.Dimensions), "vec");
            var res = default (V); 
			res[0] = Column0.X * vec[0] + Column1.X * vec[1] + Column2.X * vec[2] + Column3.X * vec[3];
			res[1] = Column0.Y * vec[0] + Column1.Y * vec[1] + Column2.Y * vec[2] + Column3.Y * vec[3];
			res[2] = Column0.Z * vec[0] + Column1.Z * vec[1] + Column2.Z * vec[2] + Column3.Z * vec[3];
			res[3] = Column0.W * vec[0] + Column1.W * vec[1] + Column2.W * vec[2] + Column3.W * vec[3];
			return res;
        }

		public bool Equals (Mat4 other)
		{
			return Column0 == other.Column0 && Column1 == other.Column1 && Column2 == other.Column2 && Column3 == other.Column3;
		}

		public override bool Equals (object obj)
		{
            return obj is Mat4 && Equals ((Mat4)obj);
		}

        public override int GetHashCode ()
        {
			return Column0.GetHashCode () ^ Column1.GetHashCode () ^ Column2.GetHashCode () ^ Column3.GetHashCode ();
        }

        public override string ToString ()
        {
            var sb = new StringBuilder ();
            sb.AppendLine ();
            for (int r = 0; r < 4; r++)
            {
                sb.Append ("[");
                for (int c = 0; c < 4; c++)
                    sb.AppendFormat (" {0}", this[c, r]);
                sb.AppendLine (" ]");
            }
            return sb.ToString ();
        }

        public static Mat4 operator - (Mat4 left, Mat4 right)
        {
            return left.Subtract (right);
        }

        public static Mat4 operator * (float scalar, Mat4 mat)
        {
            return mat.Multiply (scalar);
        }

        public static Mat4 operator * (Mat4 mat, float scalar)
        {
            return mat.Multiply (scalar);
        }

        public static Mat4 operator * (Mat4 left, Mat4 right)
        {
			return new Mat4 (
				left.Column0.X * right.Column0.X + left.Column1.X * right.Column0.Y + left.Column2.X * right.Column0.Z + left.Column3.X * right.Column0.W,
				left.Column0.Y * right.Column0.X + left.Column1.Y * right.Column0.Y + left.Column2.Y * right.Column0.Z + left.Column3.Y * right.Column0.W,
				left.Column0.Z * right.Column0.X + left.Column1.Z * right.Column0.Y + left.Column2.Z * right.Column0.Z + left.Column3.Z * right.Column0.W,
				left.Column0.W * right.Column0.X + left.Column1.W * right.Column0.Y + left.Column2.W * right.Column0.Z + left.Column3.W * right.Column0.W,
				left.Column0.X * right.Column1.X + left.Column1.X * right.Column1.Y + left.Column2.X * right.Column1.Z + left.Column3.X * right.Column1.W,
				left.Column0.Y * right.Column1.X + left.Column1.Y * right.Column1.Y + left.Column2.Y * right.Column1.Z + left.Column3.Y * right.Column1.W,
				left.Column0.Z * right.Column1.X + left.Column1.Z * right.Column1.Y + left.Column2.Z * right.Column1.Z + left.Column3.Z * right.Column1.W,
				left.Column0.W * right.Column1.X + left.Column1.W * right.Column1.Y + left.Column2.W * right.Column1.Z + left.Column3.W * right.Column1.W,
				left.Column0.X * right.Column2.X + left.Column1.X * right.Column2.Y + left.Column2.X * right.Column2.Z + left.Column3.X * right.Column2.W,
				left.Column0.Y * right.Column2.X + left.Column1.Y * right.Column2.Y + left.Column2.Y * right.Column2.Z + left.Column3.Y * right.Column2.W,
				left.Column0.Z * right.Column2.X + left.Column1.Z * right.Column2.Y + left.Column2.Z * right.Column2.Z + left.Column3.Z * right.Column2.W,
				left.Column0.W * right.Column2.X + left.Column1.W * right.Column2.Y + left.Column2.W * right.Column2.Z + left.Column3.W * right.Column2.W,
				left.Column0.X * right.Column3.X + left.Column1.X * right.Column3.Y + left.Column2.X * right.Column3.Z + left.Column3.X * right.Column3.W,
				left.Column0.Y * right.Column3.X + left.Column1.Y * right.Column3.Y + left.Column2.Y * right.Column3.Z + left.Column3.Y * right.Column3.W,
				left.Column0.Z * right.Column3.X + left.Column1.Z * right.Column3.Y + left.Column2.Z * right.Column3.Z + left.Column3.Z * right.Column3.W,
				left.Column0.W * right.Column3.X + left.Column1.W * right.Column3.Y + left.Column2.W * right.Column3.Z + left.Column3.W * right.Column3.W);
        }

        public static Vec4 operator * (Mat4 mat, Vec4 vec)
        {
			return new Vec4 (
				mat.Column0.X * vec.X + mat.Column1.X * vec.Y + mat.Column2.X * vec.Z + mat.Column3.X * vec.W,
				mat.Column0.Y * vec.X + mat.Column1.Y * vec.Y + mat.Column2.Y * vec.Z + mat.Column3.Y * vec.W,
				mat.Column0.Z * vec.X + mat.Column1.Z * vec.Y + mat.Column2.Z * vec.Z + mat.Column3.Z * vec.W,
				mat.Column0.W * vec.X + mat.Column1.W * vec.Y + mat.Column2.W * vec.Z + mat.Column3.W * vec.W);
        }

        public static Mat4 operator / (Mat4 mat, float scalar)
        {
            return mat.Divide (scalar);
        }

        public static Mat4 operator + (Mat4 left, Mat4 right)
        {
            return left.Add (right);
        }

        public static bool operator == (Mat4 left, Mat4 right)
        {
            return left.Equals (right);
        }

        public static bool operator != (Mat4 left, Mat4 right)
        {
            return !left.Equals (right);
        }
	}
}
 
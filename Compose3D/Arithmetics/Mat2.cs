﻿namespace Compose3D.Arithmetics
{
    using System;
    using System.Text;

    public struct Mat2 : IMat<Mat2, float>, IEquatable<Mat2>
    { 
		public Vec2 Column0; 
		public Vec2 Column1; 

		public Mat2 (Vec2 column0, Vec2 column1)
		{
			Column0 = column0; 
			Column1 = column1; 
		}

 		public Mat2 (float value)
		{	
			Column0 = new Vec2 (value, 0); 
			Column1 = new Vec2 (0, value); 
		}

 		public Mat2 (
			float m00, float m01, 
			float m10, float m11)
		{	
			Column0 = new Vec2 (m00, m01); 
			Column1 = new Vec2 (m10, m11); 
		}

 		public int Columns
		{
			get { return 2; }
		}

		public int Rows
		{
			get { return 2; }
		}

		public Vec2 this[int column]
		{
			get
			{
				switch (column)
				{	         
					case 0: return Column0;          
					case 1: return Column1; 
			        default: throw new ArgumentOutOfRangeException("column");
				}
			} 
			set
			{
				switch (column)
				{	         
					case 0: Column0 = value; break;          
					case 1: Column1 = value; break; 
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
					
		public Mat2 Add (Mat2 other)
		{
			return new Mat2 (Column0 + other.Column0, Column1 + other.Column1);
		}

		public Mat2 Subtract (Mat2 other)
		{
			return new Mat2 (Column0 - other.Column0, Column1 - other.Column1);
		}

		public Mat2 Multiply (float scalar)
		{
			return new Mat2 (Column0 * scalar, Column1 * scalar);
		}

		public Mat2 Divide (float scalar)
		{
			return new Mat2 (Column0 / scalar, Column1 / scalar);
		}

        public V Multiply<V> (V vec) where V : struct, IVec<V, float>, IEquatable<V>
        {
            if (vec.Dimensions != Columns)
                throw new ArgumentException (
					string.Format ("Cannot multiply {0}x{1} matrix with {2}D vector", Columns, Rows, vec.Dimensions), "vec");
            var res = default (V); 
			res[0] = Column0.X * vec[0] + Column1.X * vec[1];
			res[1] = Column0.Y * vec[0] + Column1.Y * vec[1];
			return res;
        }

		public bool Equals (Mat2 other)
		{
			return Column0 == other.Column0 && Column1 == other.Column1;
		}

		public override bool Equals (object obj)
		{
            return obj is Mat2 && Equals ((Mat2)obj);
		}

        public override int GetHashCode ()
        {
			return Column0.GetHashCode () ^ Column1.GetHashCode ();
        }

        public override string ToString ()
        {
            var sb = new StringBuilder ();
            sb.AppendLine ();
            for (int r = 0; r < 2; r++)
            {
                sb.Append ("[");
                for (int c = 0; c < 2; c++)
                    sb.AppendFormat (" {0}", this[c, r]);
                sb.AppendLine (" ]");
            }
            return sb.ToString ();
        }

        public static Mat2 operator - (Mat2 left, Mat2 right)
        {
            return left.Subtract (right);
        }

        public static Mat2 operator * (float scalar, Mat2 mat)
        {
            return mat.Multiply (scalar);
        }

        public static Mat2 operator * (Mat2 mat, float scalar)
        {
            return mat.Multiply (scalar);
        }

        public static Mat2 operator * (Mat2 left, Mat2 right)
        {
			return new Mat2 (
				left.Column0.X * right.Column0.X + left.Column1.X * right.Column0.Y,
				left.Column0.Y * right.Column0.X + left.Column1.Y * right.Column0.Y,
				left.Column0.X * right.Column1.X + left.Column1.X * right.Column1.Y,
				left.Column0.Y * right.Column1.X + left.Column1.Y * right.Column1.Y);
        }

        public static Vec2 operator * (Mat2 mat, Vec2 vec)
        {
			return new Vec2 (
				mat.Column0.X * vec.X + mat.Column1.X * vec.Y,
				mat.Column0.Y * vec.X + mat.Column1.Y * vec.Y);
        }

        public static Mat2 operator / (Mat2 mat, float scalar)
        {
            return mat.Divide (scalar);
        }

        public static Mat2 operator + (Mat2 left, Mat2 right)
        {
            return left.Add (right);
        }

        public static bool operator == (Mat2 left, Mat2 right)
        {
            return left.Equals (right);
        }

        public static bool operator != (Mat2 left, Mat2 right)
        {
            return !left.Equals (right);
        }
	}
}
 
﻿namespace Compose3D.Arithmetics
{
    using System;
    using System.Text;

    public struct Vec2 : IVec<Vec2, float>
    { 
		public float X; 
		public float Y; 

		public Vec2 (float x, float y)
		{	
			X = x; 
			Y = y; 
		}

 		public Vec2 (float value)
		{	
			X = value; 
			Y = value; 
		}

 		public Vec2 (Vec2 vec)
		{	
			X = vec.X; 
			Y = vec.Y; 
		}

 		public Vec2 (Vec3 vec)
		{	
			X = vec.X; 
			Y = vec.Y; 
		}

 		public Vec2 (Vec4 vec)
		{	
			X = vec.X; 
			Y = vec.Y; 
		}

 		public Vec2 Negate ()
		{
			return new Vec2 (-X, -Y);
		}

		public Vec2 Add (Vec2 other)
		{
			return new Vec2 (X + other.X, Y + other.Y);
		}

		public Vec2 Subtract (Vec2 other)
		{
			return new Vec2 (X - other.X, Y - other.Y);
		}

		public Vec2 Multiply (Vec2 other)
		{
			return new Vec2 (X * other.X, Y * other.Y);
		}

		public Vec2 Multiply (float scalar)
		{
			return new Vec2 (X * scalar, Y * scalar);
		}

		public Vec2 Divide (float scalar)
		{
			return new Vec2 (X / scalar, Y / scalar);
		}

		public float Dot (Vec2 other)
		{
			return X * other.X + Y * other.Y;
		}

		public bool Equals (Vec2 other)
		{
			return X == other.X && Y == other.Y;
		}

		public int Dimensions
		{
			get { return 2; }
		}

		public float this[int index]
		{
			get
			{
				switch (index)
				{	         
					case 0: return X;          
					case 1: return Y; 
			        default: throw new ArgumentOutOfRangeException("index");
				}
			} 
			set
			{
				switch (index)
				{	         
					case 0: X = value; break;          
					case 1: Y = value; break; 
			        default: throw new ArgumentOutOfRangeException("index");
				}
			} 
		}
					
		public Vec2 this[Coord x, Coord y]
		{
			get { return new Vec2 (this[(int)x], this[(int)y]); }
			set
			{
				this[(int)x] = value.X; 
				this[(int)y] = value.Y; 
			}
		}

		public float LengthSquared
		{
			get { return X * X + Y * Y; }
		}

		public float Length
		{
			get { return (float)Math.Sqrt (LengthSquared); }
		}

		public Vec2 Normalized
		{
			get { return Divide (Length); }
		}

		public override bool Equals (object obj)
		{
            return obj is Vec2 && Equals ((Vec2)obj);
		}

        public override int GetHashCode ()
        {
			return X.GetHashCode () ^ Y.GetHashCode ();
        }

        public override string ToString ()
        {
            var sb = new StringBuilder ("[");
            for (int i = 0; i < 2; i++)
                sb.AppendFormat (" {0}", this[i].ToString ());
            sb.Append (" ]");
            return sb.ToString ();
        }

        public static Vec2 operator - (Vec2 vec)
        {
            return vec.Negate ();
        }

        public static Vec2 operator - (Vec2 left, Vec2 right)
        {
            return left.Subtract (right);
        }

        public static Vec2 operator * (float scalar, Vec2 vec)
        {
            return vec.Multiply (scalar);
        }

        public static Vec2 operator * (Vec2 vec, float scalar)
        {
            return vec.Multiply (scalar);
        }

        public static Vec2 operator * (Vec2 vec, Vec2 scale)
        {
            return vec.Multiply (scale);
        }

        public static Vec2 operator / (Vec2 vec, float scalar)
        {
            return vec.Divide (scalar);
        }

        public static Vec2 operator + (Vec2 left, Vec2 right)
        {
            return left.Add (right);
        }

        public static bool operator == (Vec2 left, Vec2 right)
        {
            return left.Equals (right);
        }

        public static bool operator != (Vec2 left, Vec2 right)
        {
            return !left.Equals (right);
        }
    }
}  
﻿namespace Compose3D.Arithmetics
{
    using System;
    using System.Text;

    public struct Vec4 : IVec<Vec4, float>
    { 
		public float X; 
		public float Y; 
		public float Z; 
		public float W; 

		public Vec4 (float x, float y, float z, float w)
		{	
			X = x; 
			Y = y; 
			Z = z; 
			W = w; 
		}

 		public Vec4 (float value)
		{	
			X = value; 
			Y = value; 
			Z = value; 
			W = value; 
		}

 		public Vec4 (Vec2 vec, float z, float w)
		{	
			X = vec.X; 
			Y = vec.Y; 
			Z = z; 
			W = w; 
		}

 		public Vec4 (Vec3 vec, float w)
		{	
			X = vec.X; 
			Y = vec.Y; 
			Z = vec.Z; 
			W = w; 
		}

 		public Vec4 (Vec4 vec)
		{	
			X = vec.X; 
			Y = vec.Y; 
			Z = vec.Z; 
			W = vec.W; 
		}

 		public Vec4 Negate ()
		{
			return new Vec4 (-X, -Y, -Z, -W);
		}

		public Vec4 Add (Vec4 other)
		{
			return new Vec4 (X + other.X, Y + other.Y, Z + other.Z, W + other.W);
		}

		public Vec4 Subtract (Vec4 other)
		{
			return new Vec4 (X - other.X, Y - other.Y, Z - other.Z, W - other.W);
		}

		public Vec4 Multiply (Vec4 other)
		{
			return new Vec4 (X * other.X, Y * other.Y, Z * other.Z, W * other.W);
		}

		public Vec4 Multiply (float scalar)
		{
			return new Vec4 (X * scalar, Y * scalar, Z * scalar, W * scalar);
		}

		public Vec4 Divide (float scalar)
		{
			return new Vec4 (X / scalar, Y / scalar, Z / scalar, W / scalar);
		}

		public float Dot (Vec4 other)
		{
			return X * other.X + Y * other.Y + Z * other.Z + W * other.W;
		}

		public bool Equals (Vec4 other)
		{
			return X == other.X && Y == other.Y && Z == other.Z && W == other.W;
		}

		public int Dimensions
		{
			get { return 4; }
		}

		public float this[int index]
		{
			get
			{
				switch (index)
				{	         
					case 0: return X;          
					case 1: return Y;          
					case 2: return Z;          
					case 3: return W; 
			        default: throw new ArgumentOutOfRangeException("index");
				}
			} 
			set
			{
				switch (index)
				{	         
					case 0: X = value; break;          
					case 1: Y = value; break;          
					case 2: Z = value; break;          
					case 3: W = value; break; 
			        default: throw new ArgumentOutOfRangeException("index");
				}
			} 
		}
					
		public Vec4 this[Coord x, Coord y, Coord z, Coord w]
		{
			get { return new Vec4 (this[(int)x], this[(int)y], this[(int)z], this[(int)w]); }
			set
			{
				this[(int)x] = value.X; 
				this[(int)y] = value.Y; 
				this[(int)z] = value.Z; 
				this[(int)w] = value.W; 
			}
		}

		public Vec3 this[Coord x, Coord y, Coord z]
		{
			get { return new Vec3 (this[(int)x], this[(int)y], this[(int)z]); }
			set
			{
				this[(int)x] = value.X; 
				this[(int)y] = value.Y; 
				this[(int)z] = value.Z; 
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
			get { return X * X + Y * Y + Z * Z + W * W; }
		}

		public float Length
		{
			get { return (float)Math.Sqrt (LengthSquared); }
		}

		public Vec4 Normalized
		{
			get { return Divide (Length); }
		}

		public override bool Equals (object obj)
		{
            return obj is Vec4 && Equals ((Vec4)obj);
		}

        public override int GetHashCode ()
        {
			return X.GetHashCode () ^ Y.GetHashCode () ^ Z.GetHashCode () ^ W.GetHashCode ();
        }

        public override string ToString ()
        {
            var sb = new StringBuilder ("[");
            for (int i = 0; i < 4; i++)
                sb.AppendFormat (" {0}", this[i].ToString ());
            sb.Append (" ]");
            return sb.ToString ();
        }

        public static Vec4 operator - (Vec4 vec)
        {
            return vec.Negate ();
        }

        public static Vec4 operator - (Vec4 left, Vec4 right)
        {
            return left.Subtract (right);
        }

        public static Vec4 operator * (float scalar, Vec4 vec)
        {
            return vec.Multiply (scalar);
        }

        public static Vec4 operator * (Vec4 vec, float scalar)
        {
            return vec.Multiply (scalar);
        }

        public static Vec4 operator * (Vec4 vec, Vec4 scale)
        {
            return vec.Multiply (scale);
        }

        public static Vec4 operator / (Vec4 vec, float scalar)
        {
            return vec.Divide (scalar);
        }

        public static Vec4 operator + (Vec4 left, Vec4 right)
        {
            return left.Add (right);
        }

        public static bool operator == (Vec4 left, Vec4 right)
        {
            return left.Equals (right);
        }

        public static bool operator != (Vec4 left, Vec4 right)
        {
            return !left.Equals (right);
        }
    }
} 
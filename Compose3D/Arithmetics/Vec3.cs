namespace Compose3D.Arithmetics
{
    using System;
    using System.Text;

    public struct Vec3 : IVec<Vec3, float>
    { 
		public float X; 
		public float Y; 
		public float Z; 

		public Vec3 (float x, float y, float z)
		{	
			X = x; 
			Y = y; 
			Z = z; 
		}

 		public Vec3 (float value)
		{	
			X = value; 
			Y = value; 
			Z = value; 
		}

 		public Vec3 (Vec2 vec, float z)
		{	
			X = vec.X; 
			Y = vec.Y; 
			Z = z; 
		}

 		public Vec3 (Vec3 vec)
		{	
			X = vec.X; 
			Y = vec.Y; 
			Z = vec.Z; 
		}

 		public Vec3 (Vec4 vec)
		{	
			X = vec.X; 
			Y = vec.Y; 
			Z = vec.Z; 
		}

 		public Vec3 Negate ()
		{
			return new Vec3 (-X, -Y, -Z);
		}

		public Vec3 Add (Vec3 other)
		{
			return new Vec3 (X + other.X, Y + other.Y, Z + other.Z);
		}

		public Vec3 Subtract (Vec3 other)
		{
			return new Vec3 (X - other.X, Y - other.Y, Z - other.Z);
		}

		public Vec3 Multiply (Vec3 other)
		{
			return new Vec3 (X * other.X, Y * other.Y, Z * other.Z);
		}

		public Vec3 Multiply (float scalar)
		{
			return new Vec3 (X * scalar, Y * scalar, Z * scalar);
		}

		public Vec3 Divide (float scalar)
		{
			return new Vec3 (X / scalar, Y / scalar, Z / scalar);
		}

		public float Dot (Vec3 other)
		{
			return X * other.X + Y * other.Y + Z * other.Z;
		}

		public bool Equals (Vec3 other)
		{
			return X == other.X && Y == other.Y && Z == other.Z;
		}

		public int Dimensions
		{
			get { return 3; }
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
			        default: throw new ArgumentOutOfRangeException("index");
				}
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
			get { return X * X + Y * Y + Z * Z; }
		}

		public float Length
		{
			get { return (float)Math.Sqrt (LengthSquared); }
		}

		public Vec3 Normalized
		{
			get { return Divide (Length); }
		}

		public override bool Equals (object obj)
		{
            return obj is Vec3 && Equals ((Vec3)obj);
		}

        public override int GetHashCode ()
        {
			return X.GetHashCode () ^ Y.GetHashCode () ^ Z.GetHashCode ();
        }

        public override string ToString ()
        {
            var sb = new StringBuilder ("[");
            for (int i = 0; i < 3; i++)
                sb.AppendFormat (" {0}", this[i].ToString ());
            sb.Append (" ]");
            return sb.ToString ();
        }

        public static Vec3 operator - (Vec3 vec)
        {
            return vec.Negate ();
        }

        public static Vec3 operator - (Vec3 left, Vec3 right)
        {
            return left.Subtract (right);
        }

        public static Vec3 operator * (float scalar, Vec3 vec)
        {
            return vec.Multiply (scalar);
        }

        public static Vec3 operator * (Vec3 vec, float scalar)
        {
            return vec.Multiply (scalar);
        }

        public static Vec3 operator * (Vec3 vec, Vec3 scale)
        {
            return vec.Multiply (scale);
        }

        public static Vec3 operator / (Vec3 vec, float scalar)
        {
            return vec.Divide (scalar);
        }

        public static Vec3 operator + (Vec3 left, Vec3 right)
        {
            return left.Add (right);
        }

        public static bool operator == (Vec3 left, Vec3 right)
        {
            return left.Equals (right);
        }

        public static bool operator != (Vec3 left, Vec3 right)
        {
            return !left.Equals (right);
        }
    }
} 
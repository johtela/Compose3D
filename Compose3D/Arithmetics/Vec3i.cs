namespace Compose3D.Arithmetics
{
    using System;
    using System.Text;
	using GLTypes;

	[GLType ("ivec3")]
    public struct Vec3i : IVec<Vec3i, int>
    { 
		public int X; 
		public int Y; 
		public int Z; 

		[GLConstructor ("ivec3 ({0})")]
		public Vec3i (int x, int y, int z)
		{	
			X = x; 
			Y = y; 
			Z = z; 
		}

		[GLConstructor ("ivec3 ({0})")]
		public Vec3i (int value)
		{	
			X = value; 
			Y = value; 
			Z = value; 
		}

		[GLConstructor ("ivec3 ({0})")]
		public Vec3i (Vec2i vec, int z)
		{	
			X = vec.X; 
			Y = vec.Y; 
			Z = z; 
		}

		[GLConstructor ("ivec3 ({0})")]
		public Vec3i (Vec3i vec)
		{	
			X = vec.X; 
			Y = vec.Y; 
			Z = vec.Z; 
		}

		[GLConstructor ("ivec3 ({0})")]
		public Vec3i (Vec4i vec)
		{	
			X = vec.X; 
			Y = vec.Y; 
			Z = vec.Z; 
		}

		[GLUnaryOperator ("-{0}")]
		public Vec3i Negate ()
		{
			return new Vec3i (-X, -Y, -Z);
		}

		[GLBinaryOperator ("{0} + {1}")]
		public Vec3i Add (Vec3i other)
		{
			return new Vec3i (X + other.X, Y + other.Y, Z + other.Z);
		}

		[GLBinaryOperator ("{0} - {1}")]
		public Vec3i Subtract (Vec3i other)
		{
			return new Vec3i (X - other.X, Y - other.Y, Z - other.Z);
		}

		[GLBinaryOperator ("{0} * {1}")]
		public Vec3i Multiply (Vec3i other)
		{
			return new Vec3i (X * other.X, Y * other.Y, Z * other.Z);
		}

		[GLBinaryOperator ("{0} * {1}")]
		public Vec3i Multiply (int scalar)
		{
			return new Vec3i (X * scalar, Y * scalar, Z * scalar);
		}

		[GLBinaryOperator ("{0} / {1}")]
		public Vec3i Divide (int scalar)
		{
			return new Vec3i (X / scalar, Y / scalar, Z / scalar);
		}

		[GLFunction ("dot ({0})")]
		public int Dot (Vec3i other)
		{
			return X * other.X + Y * other.Y + Z * other.Z;
		}

		[GLFunction ("pow ({0})")]
		public Vec3i Pow (Vec3i other)
		{
			return new Vec3i (Numeric.Pow (X, other.X), Numeric.Pow (Y, other.Y), Numeric.Pow (Z, other.Z));
		}

		[GLFunction ("clamp ({0})")]
		public Vec3i Clamp (int min, int max)
		{
			return new Vec3i (X.Clamp (min, max), Y.Clamp (min, max), Z.Clamp (min, max));
		}

		[GLFunction ("reflect ({0})")]
		public Vec3i Reflect (Vec3i along)
		{
			return Subtract (along.Multiply (2 * Dot (along)));
		}

		public bool Equals (Vec3i other)
		{
			return X == other.X && Y == other.Y && Z == other.Z;
		}

		public int Dimensions
		{
			get { return 3; }
		}

		public int this[int index]
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

		public Vec3i this[Coord x, Coord y, Coord z]
		{
			get { return new Vec3i (this[(int)x], this[(int)y], this[(int)z]); }
			set
			{
				this[(int)x] = value.X; 
				this[(int)y] = value.Y; 
				this[(int)z] = value.Z; 
			}
		}

		public Vec2i this[Coord x, Coord y]
		{
			get { return new Vec2i (this[(int)x], this[(int)y]); }
			set
			{
				this[(int)x] = value.X; 
				this[(int)y] = value.Y; 
			}
		}
		
		public int LengthSquared
		{
			get { return X * X + Y * Y + Z * Z; }
		}

		[GLFunction ("length ({0})")]
		public int Length
		{
			get { return (int)Math.Sqrt (LengthSquared); }
		}

		[GLFunction ("normalize ({0})")]
		public Vec3i Normalized
		{
			get { return Divide (Length); }
		}

		public override bool Equals (object obj)
		{
            return obj is Vec3i && Equals ((Vec3i)obj);
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

		[GLUnaryOperator ("-{0}")]
        public static Vec3i operator - (Vec3i vec)
        {
            return vec.Negate ();
        }

		[GLBinaryOperator ("{0} - {1}")]
        public static Vec3i operator - (Vec3i left, Vec3i right)
        {
            return left.Subtract (right);
        }

		[GLBinaryOperator ("{0} * {1}")]
        public static Vec3i operator * (int scalar, Vec3i vec)
        {
            return vec.Multiply (scalar);
        }

		[GLBinaryOperator ("{0} * {1}")]
        public static Vec3i operator * (Vec3i vec, int scalar)
        {
            return vec.Multiply (scalar);
        }

		[GLBinaryOperator ("{0} * {1}")]
        public static Vec3i operator * (Vec3i vec, Vec3i scale)
        {
            return vec.Multiply (scale);
        }

		[GLBinaryOperator ("{0} / {1}")]
        public static Vec3i operator / (Vec3i vec, int scalar)
        {
            return vec.Divide (scalar);
        }

		[GLBinaryOperator ("{0} + {1}")]
        public static Vec3i operator + (Vec3i left, Vec3i right)
        {
            return left.Add (right);
        }

		[GLBinaryOperator ("{0} == {1}")]
        public static bool operator == (Vec3i left, Vec3i right)
        {
            return left.Equals (right);
        }

		[GLBinaryOperator ("{0} != {1}")]
        public static bool operator != (Vec3i left, Vec3i right)
        {
            return !left.Equals (right);
        }
    }
} 
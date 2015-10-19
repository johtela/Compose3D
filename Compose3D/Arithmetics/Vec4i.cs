namespace Compose3D.Arithmetics
{
    using System;
    using System.Text;
	using GLTypes;

	[GLType ("ivec4")]
    public struct Vec4i : IVec<Vec4i, int>
    { 
		public int X; 
		public int Y; 
		public int Z; 
		public int W; 

		[GLConstructor ("ivec4 ({0})")]
		public Vec4i (int x, int y, int z, int w)
		{	
			X = x; 
			Y = y; 
			Z = z; 
			W = w; 
		}

		[GLConstructor ("ivec4 ({0})")]
		public Vec4i (int value)
		{	
			X = value; 
			Y = value; 
			Z = value; 
			W = value; 
		}

		[GLConstructor ("ivec4 ({0})")]
		public Vec4i (Vec2i vec, int z, int w)
		{	
			X = vec.X; 
			Y = vec.Y; 
			Z = z; 
			W = w; 
		}

		[GLConstructor ("ivec4 ({0})")]
		public Vec4i (Vec3i vec, int w)
		{	
			X = vec.X; 
			Y = vec.Y; 
			Z = vec.Z; 
			W = w; 
		}

		[GLConstructor ("ivec4 ({0})")]
		public Vec4i (Vec4i vec)
		{	
			X = vec.X; 
			Y = vec.Y; 
			Z = vec.Z; 
			W = vec.W; 
		}

		[GLUnaryOperator ("-{0}")]
		public Vec4i Negate ()
		{
			return new Vec4i (-X, -Y, -Z, -W);
		}

		[GLBinaryOperator ("{0} + {1}")]
		public Vec4i Add (Vec4i other)
		{
			return new Vec4i (X + other.X, Y + other.Y, Z + other.Z, W + other.W);
		}

		[GLBinaryOperator ("{0} - {1}")]
		public Vec4i Subtract (Vec4i other)
		{
			return new Vec4i (X - other.X, Y - other.Y, Z - other.Z, W - other.W);
		}

		[GLBinaryOperator ("{0} * {1}")]
		public Vec4i Multiply (Vec4i other)
		{
			return new Vec4i (X * other.X, Y * other.Y, Z * other.Z, W * other.W);
		}

		[GLBinaryOperator ("{0} * {1}")]
		public Vec4i Multiply (int scalar)
		{
			return new Vec4i (X * scalar, Y * scalar, Z * scalar, W * scalar);
		}

		[GLBinaryOperator ("{0} / {1}")]
		public Vec4i Divide (int scalar)
		{
			return new Vec4i (X / scalar, Y / scalar, Z / scalar, W / scalar);
		}

		[GLFunction ("dot ({0})")]
		public int Dot (Vec4i other)
		{
			return X * other.X + Y * other.Y + Z * other.Z + W * other.W;
		}

		[GLFunction ("pow ({0})")]
		public Vec4i Pow (Vec4i other)
		{
			return new Vec4i (GLMath.Pow (X, other.X), GLMath.Pow (Y, other.Y), GLMath.Pow (Z, other.Z), GLMath.Pow (W, other.W));
		}

		[GLFunction ("clamp ({0})")]
		public Vec4i Clamp (int min, int max)
		{
			return new Vec4i (X.Clamp (min, max), Y.Clamp (min, max), Z.Clamp (min, max), W.Clamp (min, max));
		}

		[GLFunction ("reflect ({0})")]
		public Vec4i Reflect (Vec4i along)
		{
			return Subtract (along.Multiply (2 * Dot (along)));
		}

		public bool Equals (Vec4i other)
		{
			return X == other.X && Y == other.Y && Z == other.Z && W == other.W;
		}

		public int Dimensions
		{
			get { return 4; }
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

		public Vec4i this[Coord x, Coord y, Coord z, Coord w]
		{
			get { return new Vec4i (this[(int)x], this[(int)y], this[(int)z], this[(int)w]); }
			set
			{
				this[(int)x] = value.X; 
				this[(int)y] = value.Y; 
				this[(int)z] = value.Z; 
				this[(int)w] = value.W; 
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
			get { return X * X + Y * Y + Z * Z + W * W; }
		}

		[GLFunction ("length ({0})")]
		public int Length
		{
			get { return (int)Math.Sqrt (LengthSquared); }
		}

		[GLFunction ("normalize ({0})")]
		public Vec4i Normalized
		{
			get { return Divide (Length); }
		}

		public override bool Equals (object obj)
		{
            return obj is Vec4i && Equals ((Vec4i)obj);
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

		[GLUnaryOperator ("-{0}")]
        public static Vec4i operator - (Vec4i vec)
        {
            return vec.Negate ();
        }

		[GLBinaryOperator ("{0} - {1}")]
        public static Vec4i operator - (Vec4i left, Vec4i right)
        {
            return left.Subtract (right);
        }

		[GLBinaryOperator ("{0} * {1}")]
        public static Vec4i operator * (int scalar, Vec4i vec)
        {
            return vec.Multiply (scalar);
        }

		[GLBinaryOperator ("{0} * {1}")]
        public static Vec4i operator * (Vec4i vec, int scalar)
        {
            return vec.Multiply (scalar);
        }

		[GLBinaryOperator ("{0} * {1}")]
        public static Vec4i operator * (Vec4i vec, Vec4i scale)
        {
            return vec.Multiply (scale);
        }

		[GLBinaryOperator ("{0} / {1}")]
        public static Vec4i operator / (Vec4i vec, int scalar)
        {
            return vec.Divide (scalar);
        }

		[GLBinaryOperator ("{0} + {1}")]
        public static Vec4i operator + (Vec4i left, Vec4i right)
        {
            return left.Add (right);
        }

		[GLBinaryOperator ("{0} == {1}")]
        public static bool operator == (Vec4i left, Vec4i right)
        {
            return left.Equals (right);
        }

		[GLBinaryOperator ("{0} != {1}")]
        public static bool operator != (Vec4i left, Vec4i right)
        {
            return !left.Equals (right);
        }
    }
} 
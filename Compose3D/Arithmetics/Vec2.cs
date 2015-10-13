namespace Compose3D.Arithmetics
{
    using System;
    using System.Text;
	using GLTypes;

	[GLType ("vec2")]
    public struct Vec2 : IVec<Vec2, float>
    { 
		public float X; 
		public float Y; 

		[GLConstructor ("vec2 ({0})")]
		public Vec2 (float x, float y)
		{	
			X = x; 
			Y = y; 
		}

		[GLConstructor ("vec2 ({0})")]
		public Vec2 (float value)
		{	
			X = value; 
			Y = value; 
		}

		[GLConstructor ("vec2 ({0})")]
		public Vec2 (Vec2 vec)
		{	
			X = vec.X; 
			Y = vec.Y; 
		}

		[GLConstructor ("vec2 ({0})")]
		public Vec2 (Vec3 vec)
		{	
			X = vec.X; 
			Y = vec.Y; 
		}

		[GLConstructor ("vec2 ({0})")]
		public Vec2 (Vec4 vec)
		{	
			X = vec.X; 
			Y = vec.Y; 
		}

		[GLUnaryOperator ("-{0}")]
		public Vec2 Negate ()
		{
			return new Vec2 (-X, -Y);
		}

		[GLBinaryOperator ("{0} + {1}")]
		public Vec2 Add (Vec2 other)
		{
			return new Vec2 (X + other.X, Y + other.Y);
		}

		[GLBinaryOperator ("{0} - {1}")]
		public Vec2 Subtract (Vec2 other)
		{
			return new Vec2 (X - other.X, Y - other.Y);
		}

		[GLBinaryOperator ("{0} * {1}")]
		public Vec2 Multiply (Vec2 other)
		{
			return new Vec2 (X * other.X, Y * other.Y);
		}

		[GLBinaryOperator ("{0} * {1}")]
		public Vec2 Multiply (float scalar)
		{
			return new Vec2 (X * scalar, Y * scalar);
		}

		[GLBinaryOperator ("{0} / {1}")]
		public Vec2 Divide (float scalar)
		{
			return new Vec2 (X / scalar, Y / scalar);
		}

		[GLFunction ("dot ({0})")]
		public float Dot (Vec2 other)
		{
			return X * other.X + Y * other.Y;
		}

		[GLFunction ("pow ({0})")]
		public Vec2 Pow (Vec2 other)
		{
			return new Vec2 (Numeric.Pow (X, other.X), Numeric.Pow (Y, other.Y));
		}

		[GLFunction ("clamp ({0})")]
		public Vec2 Clamp (float min, float max)
		{
			return new Vec2 (X.Clamp (min, max), Y.Clamp (min, max));
		}

		[GLFunction ("reflect ({0})")]
		public Vec2 Reflect (Vec2 along)
		{
			return Subtract (along.Multiply (2 * Dot (along)));
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

		[GLFunction ("length ({0})")]
		public float Length
		{
			get { return (float)Math.Sqrt (LengthSquared); }
		}

		[GLFunction ("normalize ({0})")]
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

		[GLUnaryOperator ("-{0}")]
        public static Vec2 operator - (Vec2 vec)
        {
            return vec.Negate ();
        }

		[GLBinaryOperator ("{0} - {1}")]
        public static Vec2 operator - (Vec2 left, Vec2 right)
        {
            return left.Subtract (right);
        }

		[GLBinaryOperator ("{0} * {1}")]
        public static Vec2 operator * (float scalar, Vec2 vec)
        {
            return vec.Multiply (scalar);
        }

		[GLBinaryOperator ("{0} * {1}")]
        public static Vec2 operator * (Vec2 vec, float scalar)
        {
            return vec.Multiply (scalar);
        }

		[GLBinaryOperator ("{0} * {1}")]
        public static Vec2 operator * (Vec2 vec, Vec2 scale)
        {
            return vec.Multiply (scale);
        }

		[GLBinaryOperator ("{0} / {1}")]
        public static Vec2 operator / (Vec2 vec, float scalar)
        {
            return vec.Divide (scalar);
        }

		[GLBinaryOperator ("{0} + {1}")]
        public static Vec2 operator + (Vec2 left, Vec2 right)
        {
            return left.Add (right);
        }

		[GLBinaryOperator ("{0} == {1}")]
        public static bool operator == (Vec2 left, Vec2 right)
        {
            return left.Equals (right);
        }

		[GLBinaryOperator ("{0} != {1}")]
        public static bool operator != (Vec2 left, Vec2 right)
        {
            return !left.Equals (right);
        }
    }
} 
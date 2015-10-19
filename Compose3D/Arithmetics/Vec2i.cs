namespace Compose3D.Arithmetics
{
    using System;
    using System.Text;
	using GLTypes;

	[GLType ("ivec2")]
    public struct Vec2i : IVec<Vec2i, int>
    { 
		public int X; 
		public int Y; 
		[GLConstructor ("ivec2 ({0})")]
		public Vec2i (int x, int y)
		{	
			X = x; 
			Y = y; 
		}
		[GLConstructor ("ivec2 ({0})")]
		public Vec2i (int value)
		{	
			X = value; 
			Y = value; 
		}
		[GLConstructor ("ivec2 ({0})")]
		public Vec2i (Vec2i vec)
		{	
			X = vec.X; 
			Y = vec.Y; 
		}

		[GLConstructor ("ivec2 ({0})")]
		public Vec2i (Vec3i vec)
		{	
			X = vec.X; 
			Y = vec.Y; 
		}

		[GLConstructor ("ivec2 ({0})")]
		public Vec2i (Vec4i vec)
		{	
			X = vec.X; 
			Y = vec.Y; 
		}

		[GLUnaryOperator ("-{0}")]
		public Vec2i Negate ()
		{
			return new Vec2i (-X, -Y);
		}

		[GLBinaryOperator ("{0} + {1}")]
		public Vec2i Add (Vec2i other)
		{
			return new Vec2i (X + other.X, Y + other.Y);
		}

		[GLBinaryOperator ("{0} - {1}")]
		public Vec2i Subtract (Vec2i other)
		{
			return new Vec2i (X - other.X, Y - other.Y);
		}

		[GLBinaryOperator ("{0} * {1}")]
		public Vec2i Multiply (Vec2i other)
		{
			return new Vec2i (X * other.X, Y * other.Y);
		}

		[GLBinaryOperator ("{0} * {1}")]
		public Vec2i Multiply (int scalar)
		{
			return new Vec2i (X * scalar, Y * scalar);
		}

		[GLBinaryOperator ("{0} / {1}")]
		public Vec2i Divide (int scalar)
		{
			return new Vec2i (X / scalar, Y / scalar);
		}

		[GLFunction ("dot ({0})")]
		public int Dot (Vec2i other)
		{
			return X * other.X + Y * other.Y;
		}
		public bool Equals (Vec2i other)
		{
			return X == other.X && Y == other.Y;
		}
		public int Dimensions
		{
			get { return 2; }
		}

		public int this[int index]
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
			get { return X * X + Y * Y; }
		}

		[GLFunction ("length ({0})")]
		public int Length
		{
			get { return (int)Math.Sqrt (LengthSquared); }
		}

		[GLFunction ("normalize ({0})")]
		public Vec2i Normalized
		{
			get { return Divide (Length); }
		}

		public override bool Equals (object obj)
		{
            return obj is Vec2i && Equals ((Vec2i)obj);
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
        public static Vec2i operator - (Vec2i vec)
        {
            return vec.Negate ();
        }

		[GLBinaryOperator ("{0} - {1}")]
        public static Vec2i operator - (Vec2i left, Vec2i right)
        {
            return left.Subtract (right);
        }

		[GLBinaryOperator ("{0} * {1}")]
        public static Vec2i operator * (int scalar, Vec2i vec)
        {
            return vec.Multiply (scalar);
        }

		[GLBinaryOperator ("{0} * {1}")]
        public static Vec2i operator * (Vec2i vec, int scalar)
        {
            return vec.Multiply (scalar);
        }

		[GLBinaryOperator ("{0} * {1}")]
        public static Vec2i operator * (Vec2i vec, Vec2i scale)
        {
            return vec.Multiply (scale);
        }

		[GLBinaryOperator ("{0} / {1}")]
        public static Vec2i operator / (Vec2i vec, int scalar)
        {
            return vec.Divide (scalar);
        }

		[GLBinaryOperator ("{0} + {1}")]
        public static Vec2i operator + (Vec2i left, Vec2i right)
        {
            return left.Add (right);
        }

		[GLBinaryOperator ("{0} == {1}")]
        public static bool operator == (Vec2i left, Vec2i right)
        {
            return left.Equals (right);
        }

		[GLBinaryOperator ("{0} != {1}")]
        public static bool operator != (Vec2i left, Vec2i right)
        {
            return !left.Equals (right);
        }
    }
} 
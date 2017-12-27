namespace ComposeFX.Maths
{
    using System;
    using System.Text;
	using System.Globalization;
	using Compute;
	using Graphics;

	/// <summary>
	/// Vector stucture that is mapped to `vec2` when used in
	/// OpenGL shaders and `float2` when used in OpenCL kernels.
	/// </summary>
	[GLType ("vec2")]
	[CLType ("float2")]
    public struct Vec2 : IVec<Vec2, float>
    { 
		/// <summary>
		/// The X component of the vector.
		/// </summary>
		[GLField ("x")]
		[CLField ("x")]
        public float X; 

		/// <summary>
		/// The Y component of the vector.
		/// </summary>
		[GLField ("y")]
		[CLField ("y")]
        public float Y; 

		/// <summary>
		/// Initialize all of the components of the vector.
		/// </summary>
		[GLConstructor ("vec2 ({0})")]
		[CLConstructor ("(float2) ({0})")]
		public Vec2 (float x, float y)
		{	
			X = x; 
			Y = y; 
		}

		/// <summary>
		/// Initialize all of the components with a same value.
		/// </summary>
		[GLConstructor ("vec2 ({0})")]
		[CLConstructor ("(float2) ({0})")]
		public Vec2 (float value)
		{	
			X = value; 
			Y = value; 
		}
		/// <summary>
		/// Copy the components of the vector from another vector.
		/// </summary>
		[GLConstructor ("vec2 ({0})")]
		[CLConstructor ("(float2) ({0})")]
		public Vec2 (Vec2 vec)
		{	
			X = vec.X; 
			Y = vec.Y; 
		}

		/// <summary>
		/// Copy the components of the vector from another vector.
		/// </summary>
		[GLConstructor ("vec2 ({0})")]
		[CLConstructor ("(float2) ({0})")]
		public Vec2 (Vec3 vec)
		{	
			X = vec.X; 
			Y = vec.Y; 
		}

		/// <summary>
		/// Copy the components of the vector from another vector.
		/// </summary>
		[GLConstructor ("vec2 ({0})")]
		[CLConstructor ("(float2) ({0})")]
		public Vec2 (Vec4 vec)
		{	
			X = vec.X; 
			Y = vec.Y; 
		}

		/// <summary>
		/// Negate all of the components of the vector.
		/// </summary>
		[GLUnaryOperator ("-{0}")]
		[CLUnaryOperator ("-{0}")]
		public Vec2 Invert ()
		{
			return new Vec2 (-X, -Y);
		}

		/// <summary>
		/// Add another vector this one componentwise.
		/// </summary>
		[GLBinaryOperator ("{0} + {1}")]
		[CLBinaryOperator ("{0} + {1}")]
		public Vec2 Add (Vec2 other)
		{
			return new Vec2 (X + other.X, Y + other.Y);
		}

		/// <summary>
		/// Subtract the given vector from this one componentwise.
		/// </summary>
		[GLBinaryOperator ("{0} - {1}")]
		[CLBinaryOperator ("{0} - {1}")]
		public Vec2 Subtract (Vec2 other)
		{
			return new Vec2 (X - other.X, Y - other.Y);
		}

		/// <summary>
		/// Multiply with another vector componentwise.
		/// </summary>
		[GLBinaryOperator ("{0} * {1}")]
		[CLBinaryOperator ("{0} * {1}")]
		public Vec2 Multiply (Vec2 other)
		{
			return new Vec2 (X * other.X, Y * other.Y);
		}

		/// <summary>
		/// Multiply the components of this vector with a same scalar value.
		/// </summary>
		[GLBinaryOperator ("{0} * {1}")]
		[CLBinaryOperator ("{0} * {1}")]
		public Vec2 Multiply (float scalar)
		{
			return new Vec2 (X * scalar, Y * scalar);
		}

		/// <summary>
		/// Divide the two vectors componentwise.
		/// </summary>
		[GLBinaryOperator ("{0} / {1}")]
		[CLBinaryOperator ("{0} / {1}")]
		public Vec2 Divide (Vec2 other)
		{
			return new Vec2 (X / other.X, Y / other.Y);
		}

		/// <summary>
		/// Divide the components of this vector by a same scalar value.
		/// </summary>
		[GLBinaryOperator ("{0} / {1}")]
		[CLBinaryOperator ("{0} / {1}")]
		public Vec2 Divide (float scalar)
		{
			return new Vec2 (X / scalar, Y / scalar);
		}

		/// <summary>
		/// Calculate the dot product with another vector.
		/// </summary>
		[GLFunction ("dot ({0})")]
		[CLFunction ("dot ({0})")]
		public float Dot (Vec2 other)
		{
			return X * other.X + Y * other.Y;
		}

		/// <summary>
		/// Equality comparison with another vector.
		/// </summary>
		public bool Equals (Vec2 other)
		{
			return X == other.X && Y == other.Y;
		}

		/// <summary>
		/// Number of dimensions/components in the vector.
		/// </summary>
		public int Dimensions
		{
			get { return 2; }
		}

		/// <summary>
		/// The value of the index'th component of the vector.
		/// </summary>
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
		
		/// <summary>
		/// Swizzling of the vector returns the specified components in the specified order.
		/// </summary>
		public Vec2 this[Coord x, Coord y]
		{
			get { return new Vec2 (this[(int)x], this[(int)y]); }
			set
			{
				this[(int)x] = value.X; 
				this[(int)y] = value.Y; 
			}
		}

		/// <summary>
		/// The lengh of the vector squared. This is bit faster to calculate than the actual length
		/// because the square root operation is omitted.
		/// </summary>
		public float LengthSquared
		{
			get { return X * X + Y * Y; }
		}

		/// <summary>
		/// The lengh of the vector.
		/// </summary>
		[GLFunction ("length ({0})")]
		[CLFunction ("length ({0})")]
		public float Length
		{
			get { return (float)Math.Sqrt (LengthSquared); }
		}

		/// <summary>
		/// The normalized vector. I.e. vector with same direction, but with lenght of 1.
		/// </summary>
		[GLFunction ("normalize ({0})")]
		[CLFunction ("normalize ({0})")]
		public Vec2 Normalized
		{
			get { return Divide (Length); }
		}

		/// <summary>
		/// Equality comparison inherited from Object. It is overridden to do the comparison componentwise.
		/// </summary>
		public override bool Equals (object obj)
		{
            return obj is Vec2 && Equals ((Vec2)obj);
		}

		/// <summary>
		/// Hash code generation inherited from Object. It is overridden to calculate the hash code componentwise.
		/// </summary>
        public override int GetHashCode ()
        {
			return X.GetHashCode () ^ Y.GetHashCode ();
        }

		/// <summary>
		/// String conversion inherited from Object. Formats the vector in matrix style.
		/// I.e. components inside square brackets without commas in between.
		/// </summary>
        public override string ToString ()
        {
            var sb = new StringBuilder ("[");
            for (int i = 0; i < 2; i++)
                sb.AppendFormat (" {0}", this[i].ToString (CultureInfo.InvariantCulture));
            sb.Append (" ]");
            return sb.ToString ();
        }

		/// <summary>
		/// Negate all of the components of the vector.
		/// </summary>
		[GLUnaryOperator ("-{0}")]
		[CLUnaryOperator ("-{0}")]
        public static Vec2 operator - (Vec2 vec)
        {
            return vec.Invert ();
        }

		/// <summary>
		/// Subtracts the right vector from the left componentwise.
		/// </summary>
		[GLBinaryOperator ("{0} - {1}")]
		[CLBinaryOperator ("{0} - {1}")]
        public static Vec2 operator - (Vec2 left, Vec2 right)
        {
            return left.Subtract (right);
        }

		/// <summary>
		/// Multiply the components of the vector with a same scalar value.
		/// </summary>
		[GLBinaryOperator ("{0} * {1}")]
		[CLBinaryOperator ("{0} * {1}")]
        public static Vec2 operator * (float scalar, Vec2 vec)
        {
            return vec.Multiply (scalar);
        }

		/// <summary>
		/// Multiply the components of the vector with a same scalar value.
		/// </summary>
		[GLBinaryOperator ("{0} * {1}")]
		[CLBinaryOperator ("{0} * {1}")]
        public static Vec2 operator * (Vec2 vec, float scalar)
        {
            return vec.Multiply (scalar);
        }

		/// <summary>
		/// Multiply the two vectors componentwise.
		/// </summary>
		[GLBinaryOperator ("{0} * {1}")]
		[CLBinaryOperator ("{0} * {1}")]
        public static Vec2 operator * (Vec2 vec, Vec2 scale)
        {
            return vec.Multiply (scale);
        }

		/// <summary>
		/// Divide the components of the vector by a same scalar value.
		/// </summary>
		[GLBinaryOperator ("{0} / {1}")]
		[CLBinaryOperator ("{0} / {1}")]
        public static Vec2 operator / (Vec2 vec, float scalar)
        {
            return vec.Divide (scalar);
        }

		/// <summary>
		/// Divide the two vectors componentwise.
		/// </summary>
		[GLBinaryOperator ("{0} / {1}")]
		[CLBinaryOperator ("{0} / {1}")]
        public static Vec2 operator / (Vec2 vec, Vec2 scale)
        {
            return vec.Divide (scale);
        }

		/// <summary>
		/// Divide a scalar by a vector.
		/// </summary>
		[GLBinaryOperator ("{0} / {1}")]
		[CLBinaryOperator ("{0} / {1}")]
        public static Vec2 operator / (float scalar, Vec2 vec)
        {
            return new Vec2 (scalar).Divide (vec);
        }

		/// <summary>
		/// Add the two vectors together componentwise.
		/// </summary>
		[GLBinaryOperator ("{0} + {1}")]
		[CLBinaryOperator ("{0} + {1}")]
        public static Vec2 operator + (Vec2 left, Vec2 right)
        {
            return left.Add (right);
        }

		/// <summary>
		/// Componentwise equality comparison between the two vectors.
		/// </summary>
		[GLBinaryOperator ("{0} == {1}")]
		[CLBinaryOperator ("{0} == {1}")]
        public static bool operator == (Vec2 left, Vec2 right)
        {
            return left.Equals (right);
        }

		/// <summary>
		/// Componentwise inequality comparison between the two vectors.
		/// </summary>
		[GLBinaryOperator ("{0} != {1}")]
		[CLBinaryOperator ("{0} != {1}")]
        public static bool operator != (Vec2 left, Vec2 right)
        {
            return !left.Equals (right);
        }
    }
} 
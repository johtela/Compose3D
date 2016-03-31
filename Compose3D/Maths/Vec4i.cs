namespace Compose3D.Maths
{
    using System;
    using System.Text;
	using GLTypes;

	/// <summary>
	/// Vector stucture that is mapped to the GLSL type `ivec4` when used in
	/// shaders. All the functionality available for vectors in GLSL is 
	/// implementented in C# as well.
	/// </summary>
	[GLType ("ivec4")]
    public struct Vec4i : IVec<Vec4i, int>
    { 
		/// <summary>
		/// The X component of the vector.
		/// </summary>
		[GLField ("x")]
        public int X; 

		/// <summary>
		/// The Y component of the vector.
		/// </summary>
		[GLField ("y")]
        public int Y; 

		/// <summary>
		/// The Z component of the vector.
		/// </summary>
		[GLField ("z")]
        public int Z; 

		/// <summary>
		/// The W component of the vector.
		/// </summary>
		[GLField ("w")]
        public int W; 

		/// <summary>
		/// Initialize all of the components of the vector.
		/// </summary>
		[GLConstructor ("ivec4 ({0})")]
		public Vec4i (int x, int y, int z, int w)
		{	
			X = x; 
			Y = y; 
			Z = z; 
			W = w; 
		}

		/// <summary>
		/// Initialize all of the components with a same value.
		/// </summary>
		[GLConstructor ("ivec4 ({0})")]
		public Vec4i (int value)
		{	
			X = value; 
			Y = value; 
			Z = value; 
			W = value; 
		}

		/// <summary>
		/// Copy the components of the vector from another vector.
		/// </summary>
		[GLConstructor ("ivec4 ({0})")]
		public Vec4i (Vec2i vec, int z, int w)
		{	
			X = vec.X; 
			Y = vec.Y; 
			Z = z; 
			W = w; 
		}

		/// <summary>
		/// Copy the components of the vector from another vector.
		/// </summary>
		[GLConstructor ("ivec4 ({0})")]
		public Vec4i (Vec3i vec, int w)
		{	
			X = vec.X; 
			Y = vec.Y; 
			Z = vec.Z; 
			W = w; 
		}

		/// <summary>
		/// Copy the components of the vector from another vector.
		/// </summary>
		[GLConstructor ("ivec4 ({0})")]
		public Vec4i (Vec4i vec)
		{	
			X = vec.X; 
			Y = vec.Y; 
			Z = vec.Z; 
			W = vec.W; 
		}

		/// <summary>
		/// Negate all of the components of the vector.
		/// </summary>
		[GLUnaryOperator ("-{0}")]
		public Vec4i Invert ()
		{
			return new Vec4i (-X, -Y, -Z, -W);
		}

		/// <summary>
		/// Add another vector this one componentwise.
		/// </summary>
		[GLBinaryOperator ("{0} + {1}")]
		public Vec4i Add (Vec4i other)
		{
			return new Vec4i (X + other.X, Y + other.Y, Z + other.Z, W + other.W);
		}

		/// <summary>
		/// Subtract the given vector from this one componentwise.
		/// </summary>
		[GLBinaryOperator ("{0} - {1}")]
		public Vec4i Subtract (Vec4i other)
		{
			return new Vec4i (X - other.X, Y - other.Y, Z - other.Z, W - other.W);
		}

		/// <summary>
		/// Multiply with another vector componentwise.
		/// </summary>
		[GLBinaryOperator ("{0} * {1}")]
		public Vec4i Multiply (Vec4i other)
		{
			return new Vec4i (X * other.X, Y * other.Y, Z * other.Z, W * other.W);
		}

		/// <summary>
		/// Multiply the components of this vector with a same scalar value.
		/// </summary>
		[GLBinaryOperator ("{0} * {1}")]
		public Vec4i Multiply (int scalar)
		{
			return new Vec4i (X * scalar, Y * scalar, Z * scalar, W * scalar);
		}

		/// <summary>
		/// Divide the two vectors componentwise.
		/// </summary>
		[GLBinaryOperator ("{0} / {1}")]
		public Vec4i Divide (Vec4i other)
		{
			return new Vec4i (X / other.X, Y / other.Y, Z / other.Z, W / other.W);
		}

		/// <summary>
		/// Divide the components of this vector by a same scalar value.
		/// </summary>
		[GLBinaryOperator ("{0} / {1}")]
		public Vec4i Divide (int scalar)
		{
			return new Vec4i (X / scalar, Y / scalar, Z / scalar, W / scalar);
		}

		/// <summary>
		/// Calculate the dot product with another vector.
		/// </summary>
		[GLFunction ("dot ({0})")]
		public int Dot (Vec4i other)
		{
			return X * other.X + Y * other.Y + Z * other.Z + W * other.W;
		}

		/// <summary>
		/// Equality comparison with another vector.
		/// </summary>
		public bool Equals (Vec4i other)
		{
			return X == other.X && Y == other.Y && Z == other.Z && W == other.W;
		}


		/// <summary>
		/// Number of dimensions/components in the vector.
		/// </summary>
		public int Dimensions
		{
			get { return 4; }
		}

		/// <summary>
		/// The value of the index'th component of the vector.
		/// </summary>
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

		
		/// <summary>
		/// Swizzling of the vector returns the specified components in the specified order.
		/// </summary>
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

		
		/// <summary>
		/// Swizzling of the vector returns the specified components in the specified order.
		/// </summary>
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

		
		/// <summary>
		/// Swizzling of the vector returns the specified components in the specified order.
		/// </summary>
		public Vec2i this[Coord x, Coord y]
		{
			get { return new Vec2i (this[(int)x], this[(int)y]); }
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
		public int LengthSquared
		{
			get { return X * X + Y * Y + Z * Z + W * W; }
		}

		/// <summary>
		/// The lengh of the vector.
		/// </summary>
		[GLFunction ("length ({0})")]
		public int Length
		{
			get { return (int)Math.Sqrt (LengthSquared); }
		}

		/// <summary>
		/// The normalized vector. I.e. vector with same direction, but with lenght of 1.
		/// </summary>
		[GLFunction ("normalize ({0})")]
		public Vec4i Normalized
		{
			get { return Divide (Length); }
		}

		/// <summary>
		/// Equality comparison inherited from Object. It is overridden to do the comparison componentwise.
		/// </summary>
		public override bool Equals (object obj)
		{
            return obj is Vec4i && Equals ((Vec4i)obj);
		}

		/// <summary>
		/// Hash code generation inherited from Object. It is overridden to calculate the hash code componentwise.
		/// </summary>
        public override int GetHashCode ()
        {
			return X.GetHashCode () ^ Y.GetHashCode () ^ Z.GetHashCode () ^ W.GetHashCode ();
        }

		/// <summary>
		/// String conversion inherited from Object. Formats the vector in matrix style.
		/// I.e. components inside square brackets without commas in between.
		/// </summary>
        public override string ToString ()
        {
            var sb = new StringBuilder ("[");
            for (int i = 0; i < 4; i++)
                sb.AppendFormat (" {0}", this[i].ToString ());
            sb.Append (" ]");
            return sb.ToString ();
        }

		/// <summary>
		/// Negate all of the components of the vector.
		/// </summary>
		[GLUnaryOperator ("-{0}")]
        public static Vec4i operator - (Vec4i vec)
        {
            return vec.Invert ();
        }

		/// <summary>
		/// Subtracts the right vector from the left componentwise.
		/// </summary>
		[GLBinaryOperator ("{0} - {1}")]
        public static Vec4i operator - (Vec4i left, Vec4i right)
        {
            return left.Subtract (right);
        }

		/// <summary>
		/// Multiply the components of the vector with a same scalar value.
		/// </summary>
		[GLBinaryOperator ("{0} * {1}")]
        public static Vec4i operator * (int scalar, Vec4i vec)
        {
            return vec.Multiply (scalar);
        }

		/// <summary>
		/// Multiply the components of the vector with a same scalar value.
		/// </summary>
		[GLBinaryOperator ("{0} * {1}")]
        public static Vec4i operator * (Vec4i vec, int scalar)
        {
            return vec.Multiply (scalar);
        }

		/// <summary>
		/// Multiply the two vectors componentwise.
		/// </summary>
		[GLBinaryOperator ("{0} * {1}")]
        public static Vec4i operator * (Vec4i vec, Vec4i scale)
        {
            return vec.Multiply (scale);
        }

		/// <summary>
		/// Divide the components of the vector by a same scalar value.
		/// </summary>
		[GLBinaryOperator ("{0} / {1}")]
        public static Vec4i operator / (Vec4i vec, int scalar)
        {
            return vec.Divide (scalar);
        }

		/// <summary>
		/// Divide the two vectors componentwise.
		/// </summary>
		[GLBinaryOperator ("{0} / {1}")]
        public static Vec4i operator / (Vec4i vec, Vec4i scale)
        {
            return vec.Divide (scale);
        }

		/// <summary>
		/// Add the two vectors together componentwise.
		/// </summary>
		[GLBinaryOperator ("{0} + {1}")]
        public static Vec4i operator + (Vec4i left, Vec4i right)
        {
            return left.Add (right);
        }

		/// <summary>
		/// Componentwise equality comparison between the two vectors.
		/// </summary>
		[GLBinaryOperator ("{0} == {1}")]
        public static bool operator == (Vec4i left, Vec4i right)
        {
            return left.Equals (right);
        }

		/// <summary>
		/// Componentwise inequality comparison between the two vectors.
		/// </summary>
		[GLBinaryOperator ("{0} != {1}")]
        public static bool operator != (Vec4i left, Vec4i right)
        {
            return !left.Equals (right);
        }
    }
} 
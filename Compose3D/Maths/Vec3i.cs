namespace Compose3D.Maths
{
    using System;
    using System.Text;
	using GLTypes;

	/// <summary>
	/// Vector stucture that is mapped to the GLSL type `ivec3` when used in
	/// shaders. All the functionality available for vectors in GLSL is 
	/// implementented in C# as well.
	/// </summary>
	[GLType ("ivec3")]
    public struct Vec3i : IVec<Vec3i, int>
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
		/// Initialize all of the components of the vector.
		/// </summary>
		[GLConstructor ("ivec3 ({0})")]
		public Vec3i (int x, int y, int z)
		{	
			X = x; 
			Y = y; 
			Z = z; 
		}

		/// <summary>
		/// Initialize all of the components with a same value.
		/// </summary>
		[GLConstructor ("ivec3 ({0})")]
		public Vec3i (int value)
		{	
			X = value; 
			Y = value; 
			Z = value; 
		}
		/// <summary>
		/// Copy the components of the vector from another vector.
		/// </summary>
		[GLConstructor ("ivec3 ({0})")]
		public Vec3i (Vec2i vec, int z)
		{	
			X = vec.X; 
			Y = vec.Y; 
			Z = z; 
		}

		/// <summary>
		/// Copy the components of the vector from another vector.
		/// </summary>
		[GLConstructor ("ivec3 ({0})")]
		public Vec3i (Vec3i vec)
		{	
			X = vec.X; 
			Y = vec.Y; 
			Z = vec.Z; 
		}

		/// <summary>
		/// Copy the components of the vector from another vector.
		/// </summary>
		[GLConstructor ("ivec3 ({0})")]
		public Vec3i (Vec4i vec)
		{	
			X = vec.X; 
			Y = vec.Y; 
			Z = vec.Z; 
		}

		/// <summary>
		/// Negate all of the components of the vector.
		/// </summary>
		[GLUnaryOperator ("-{0}")]
		public Vec3i Invert ()
		{
			return new Vec3i (-X, -Y, -Z);
		}

		/// <summary>
		/// Add another vector this one componentwise.
		/// </summary>
		[GLBinaryOperator ("{0} + {1}")]
		public Vec3i Add (Vec3i other)
		{
			return new Vec3i (X + other.X, Y + other.Y, Z + other.Z);
		}

		/// <summary>
		/// Subtract the given vector from this one componentwise.
		/// </summary>
		[GLBinaryOperator ("{0} - {1}")]
		public Vec3i Subtract (Vec3i other)
		{
			return new Vec3i (X - other.X, Y - other.Y, Z - other.Z);
		}

		/// <summary>
		/// Multiply with another vector componentwise.
		/// </summary>
		[GLBinaryOperator ("{0} * {1}")]
		public Vec3i Multiply (Vec3i other)
		{
			return new Vec3i (X * other.X, Y * other.Y, Z * other.Z);
		}

		/// <summary>
		/// Multiply the components of this vector with a same scalar value.
		/// </summary>
		[GLBinaryOperator ("{0} * {1}")]
		public Vec3i Multiply (int scalar)
		{
			return new Vec3i (X * scalar, Y * scalar, Z * scalar);
		}

		/// <summary>
		/// Divide the components of this vector by a same scalar value.
		/// </summary>
		[GLBinaryOperator ("{0} / {1}")]
		public Vec3i Divide (int scalar)
		{
			return new Vec3i (X / scalar, Y / scalar, Z / scalar);
		}

		/// <summary>
		/// Calculate the dot product with another vector.
		/// </summary>
		[GLFunction ("dot ({0})")]
		public int Dot (Vec3i other)
		{
			return X * other.X + Y * other.Y + Z * other.Z;
		}

		/// <summary>
		/// Equality comparison with another vector.
		/// </summary>
		public bool Equals (Vec3i other)
		{
			return X == other.X && Y == other.Y && Z == other.Z;
		}

		/// <summary>
		/// Number of dimensions/components in the vector.
		/// </summary>
		public int Dimensions
		{
			get { return 3; }
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
			get { return X * X + Y * Y + Z * Z; }
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
		public Vec3i Normalized
		{
			get { return Divide (Length); }
		}

		/// <summary>
		/// Equality comparison inherited from Object. It is overridden to do the comparison componentwise.
		/// </summary>
		public override bool Equals (object obj)
		{
            return obj is Vec3i && Equals ((Vec3i)obj);
		}

		/// <summary>
		/// Hash code generation inherited from Object. It is overridden to calculate the hash code componentwise.
		/// </summary>
        public override int GetHashCode ()
        {
			return X.GetHashCode () ^ Y.GetHashCode () ^ Z.GetHashCode ();
        }

		/// <summary>
		/// String conversion inherited from Object. Formats the vector in matrix style.
		/// I.e. components inside square brackets without commas in between.
		/// </summary>
        public override string ToString ()
        {
            var sb = new StringBuilder ("[");
            for (int i = 0; i < 3; i++)
                sb.AppendFormat (" {0}", this[i].ToString ());
            sb.Append (" ]");
            return sb.ToString ();
        }

		/// <summary>
		/// Negate all of the components of the vector.
		/// </summary>
		[GLUnaryOperator ("-{0}")]
        public static Vec3i operator - (Vec3i vec)
        {
            return vec.Invert ();
        }

		/// <summary>
		/// Subtracts the right vector from the left componentwise.
		/// </summary>
		[GLBinaryOperator ("{0} - {1}")]
        public static Vec3i operator - (Vec3i left, Vec3i right)
        {
            return left.Subtract (right);
        }

		/// <summary>
		/// Multiply the components of the vector with a same scalar value.
		/// </summary>
		[GLBinaryOperator ("{0} * {1}")]
        public static Vec3i operator * (int scalar, Vec3i vec)
        {
            return vec.Multiply (scalar);
        }

		/// <summary>
		/// Multiply the components of the vector with a same scalar value.
		/// </summary>
		[GLBinaryOperator ("{0} * {1}")]
        public static Vec3i operator * (Vec3i vec, int scalar)
        {
            return vec.Multiply (scalar);
        }

		/// <summary>
		/// Multiply the two vectors componentwise.
		/// </summary>
		[GLBinaryOperator ("{0} * {1}")]
        public static Vec3i operator * (Vec3i vec, Vec3i scale)
        {
            return vec.Multiply (scale);
        }

		/// <summary>
		/// Divide the components of the vector by a same scalar value.
		/// </summary>
		[GLBinaryOperator ("{0} / {1}")]
        public static Vec3i operator / (Vec3i vec, int scalar)
        {
            return vec.Divide (scalar);
        }

		/// <summary>
		/// Add the two vectors together componentwise.
		/// </summary>
		[GLBinaryOperator ("{0} + {1}")]
        public static Vec3i operator + (Vec3i left, Vec3i right)
        {
            return left.Add (right);
        }

		/// <summary>
		/// Componentwise equality comparison between the two vectors.
		/// </summary>
		[GLBinaryOperator ("{0} == {1}")]
        public static bool operator == (Vec3i left, Vec3i right)
        {
            return left.Equals (right);
        }

		/// <summary>
		/// Componentwise inequality comparison between the two vectors.
		/// </summary>
		[GLBinaryOperator ("{0} != {1}")]
        public static bool operator != (Vec3i left, Vec3i right)
        {
            return !left.Equals (right);
        }
    }
} 
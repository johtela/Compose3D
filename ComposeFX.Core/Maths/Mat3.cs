namespace ComposeFX.Maths
{
    using System;
    using System.Text;
	using Graphics;

	/// <summary>
	/// Matrix with 3 columns and rows.
	/// </summary>
	/// Matrices are defined in column-major way as in GLSL. 
	[GLType ("mat3")]
    public struct Mat3 : ISquareMat<Mat3, float>
    { 
		/// <summary>
		/// Column 0 of the matrix.
		/// </summary>
		public Vec3 Column0; 
		/// <summary>
		/// Column 1 of the matrix.
		/// </summary>
		public Vec3 Column1; 
		/// <summary>
		/// Column 2 of the matrix.
		/// </summary>
		public Vec3 Column2; 

		/// <summary>
		/// Initialize a matrix given its columns.
		/// </summary>
		[GLConstructor ("mat3 ({0})")]
		public Mat3 (Vec3 column0, Vec3 column1, Vec3 column2)
		{
			Column0 = column0; 
			Column1 = column1; 
			Column2 = column2; 
		}

 		/// <summary>
		/// Initialize a matrix using the elements of another matrix.
		/// If source matrix is smaller than the created one, unspecified
		/// elements are initialized to zero.
		/// </summary>
		[GLConstructor ("mat3 ({0})")]
		public Mat3 (Mat2 mat)
		{	
			Column0 = new Vec3 (mat.Column0, 0);
			Column1 = new Vec3 (mat.Column1, 0);
			Column2 = new Vec3 (0, 0, 1);
		}

		/// <summary>
		/// Initialize a matrix using the elements of another matrix.
		/// If source matrix is smaller than the created one, unspecified
		/// elements are initialized to zero.
		/// </summary>
		[GLConstructor ("mat3 ({0})")]
		public Mat3 (Mat3 mat)
		{	
			Column0 = new Vec3 (mat.Column0);
			Column1 = new Vec3 (mat.Column1);
			Column2 = new Vec3 (mat.Column2);
		}

		/// <summary>
		/// Initialize a matrix using the elements of another matrix.
		/// If source matrix is smaller than the created one, unspecified
		/// elements are initialized to zero.
		/// </summary>
		[GLConstructor ("mat3 ({0})")]
		public Mat3 (Mat4 mat)
		{	
			Column0 = new Vec3 (mat.Column0);
			Column1 = new Vec3 (mat.Column1);
			Column2 = new Vec3 (mat.Column2);
		}

		/// <summary>
		/// Initialize the diagonal of the matrix with a given value.
		/// The rest of the elements will be zero.
		/// </summary>
		[GLConstructor ("mat3 ({0})")]
		public Mat3 (float value)
		{	
			Column0 = new Vec3 (value, 0, 0); 
			Column1 = new Vec3 (0, value, 0); 
			Column2 = new Vec3 (0, 0, value); 
		}

 		/// <summary>
		/// Initialize all of the elements of the matrix individually.
		/// </summary>
		[GLConstructor ("mat3 ({0})")]
		public Mat3 (
			float m00, float m01, float m02, 
			float m10, float m11, float m12, 
			float m20, float m21, float m22)
		{	
			Column0 = new Vec3 (m00, m01, m02); 
			Column1 = new Vec3 (m10, m11, m12); 
			Column2 = new Vec3 (m20, m21, m22); 
		}

 		/// <summary>
		/// Number of columns in the matrix.
		/// </summary>
		public int Columns
		{
			get { return 3; }
		}

		/// <summary>
		/// Number of rows in the matrix.
		/// </summary>
		public int Rows
		{
			get { return 3; }
		}

		/// <summary>
		/// Get/set a column by its index.
		/// </summary>
		public Vec3 this[int column]
		{
			get
			{
				switch (column)
				{	         
					case 0: return Column0;          
					case 1: return Column1;          
					case 2: return Column2; 
			        default: throw new ArgumentOutOfRangeException("column");
				}
			} 
			set
			{
				switch (column)
				{	         
					case 0: Column0 = value; break;          
					case 1: Column1 = value; break;          
					case 2: Column2 = value; break; 
			        default: throw new ArgumentOutOfRangeException("column");
				}
			} 
		}

		/// <summary>
		/// Get/set a single element in the given position.
		/// </summary>
		public float this[int column, int row]
		{
			get { return this[column][row]; }
			set 
            { 
				switch (column)
				{	         
					case 0: Column0[row] = value; break;          
					case 1: Column1[row] = value; break;          
					case 2: Column2[row] = value; break; 
			        default: throw new ArgumentOutOfRangeException("column");
				}
            }
		} 
					
		/// <summary>
		/// Add two matrices together componentwise.
		/// </summary>
		[GLBinaryOperator ("{0} + {1}")]
		public Mat3 Add (Mat3 other)
		{
			return new Mat3 (Column0 + other.Column0, Column1 + other.Column1, Column2 + other.Column2);
		}

		/// <summary>
		/// Componentwise subtraction of two matrices.
		/// </summary>
		[GLBinaryOperator ("{0} - {1}")]
		public Mat3 Subtract (Mat3 other)
		{
			return new Mat3 (Column0 - other.Column0, Column1 - other.Column1, Column2 - other.Column2);
		}

		/// <summary>
		/// Multiply the matrix by a scalar componentwise.
		/// </summary>
		[GLBinaryOperator ("{0} * {1}")]
		public Mat3 Multiply (float scalar)
		{
			return new Mat3 (Column0 * scalar, Column1 * scalar, Column2 * scalar);
		}

		/// <summary>
		/// Divide the matrix by a scalar componentwise.
		/// </summary>
		[GLBinaryOperator ("{0} / {1}")]
		public Mat3 Divide (float scalar)
		{
			return new Mat3 (Column0 / scalar, Column1 / scalar, Column2 / scalar);
		}

		/// <summary>
		/// Multiply the matrix by a vector which has as many elements as the 
		/// matrix has columns. The result is a vector with same dimensions as the
		/// vector given as argument.
		/// </summary>
        public V Multiply<V> (V vec) where V : struct, IVec<V, float>, IEquatable<V>
        {
            if (vec.Dimensions != Columns)
                throw new ArgumentException (
					string.Format ("Cannot multiply {0}x{1} matrix with {2}D vector", Columns, Rows, vec.Dimensions), "vec");
            var res = default (V); 
			res[0] = Column0.X * vec[0] + Column1.X * vec[1] + Column2.X * vec[2];
			res[1] = Column0.Y * vec[0] + Column1.Y * vec[1] + Column2.Y * vec[2];
			res[2] = Column0.Z * vec[0] + Column1.Z * vec[1] + Column2.Z * vec[2];
			return res;
        }

		/// <summary>
		/// Implementation of the <see cref="System.IEquatable{Mat3}"/> interface.
		/// </summary>
		public bool Equals (Mat3 other)
		{
			return Column0 == other.Column0 && Column1 == other.Column1 && Column2 == other.Column2;
		}

		/// <summary>
		/// The multiplication of two matrices.
		/// </summary>
		[GLBinaryOperator ("{0} * {1}")]
        public Mat3 Multiply (Mat3 mat)
        {
            return this * mat;
        }

		/// <summary>
		/// Return the matrix transposed, i.e. rows and columns exchanged.
		/// </summary>
		[GLFunction ("transpose ({0})")]
        public Mat3 Transposed
        {
            get { return Mat.Transpose<Mat3, float> (this); }
        }

		/// <summary>
		/// Return the determinant of the matrix.
		/// </summary>
		[GLFunction ("determinant ({0})")]
        public float Determinant
        {
            get { return Mat.Determinant (this); }
        }

		/// <summary>
		/// Return the inverse matrix.
		/// </summary>
		[GLFunction ("inverse ({0})")]
        public Mat3 Inverse
        {
            get { return Mat.Inverse (this); }
        }

		/// <summary>
		/// Override the Equals method to work with matrices.
		/// </summary>
		public override bool Equals (object obj)
		{
            return obj is Mat3 && Equals ((Mat3)obj);
		}

		/// <summary>
		/// Override the hash code.
		/// </summary>
        public override int GetHashCode ()
        {
			return Column0.GetHashCode () ^ Column1.GetHashCode () ^ Column2.GetHashCode ();
        }

		/// <summary>
		/// Return the matrix as formatted string.
		/// </summary>
        public override string ToString ()
        {
            var sb = new StringBuilder ();
            sb.AppendLine ();
            for (int r = 0; r < 3; r++)
            {
                sb.Append ("[");
                for (int c = 0; c < 3; c++)
                    sb.AppendFormat (" {0}", this[c, r]);
                sb.AppendLine (" ]");
            }
            return sb.ToString ();
        }

		/// <summary>
		/// Componentwise subtraction of two matrices.
		/// </summary>
		[GLBinaryOperator ("{0} - {1}")]
		public static Mat3 operator - (Mat3 left, Mat3 right)
        {
            return left.Subtract (right);
        }

		/// <summary>
		/// Multiply a matrix with a scalar componentwise.
		/// </summary>
		[GLBinaryOperator ("{0} * {1}")]
        public static Mat3 operator * (float scalar, Mat3 mat)
        {
            return mat.Multiply (scalar);
        }

		/// <summary>
		/// Multiply a matrix with a scalar componentwise.
		/// </summary>
		[GLBinaryOperator ("{0} * {1}")]
        public static Mat3 operator * (Mat3 mat, float scalar)
        {
            return mat.Multiply (scalar);
        }

		/// <summary>
		/// Multiply two matrices together using the matrix product operation.
		/// </summary>
		[GLBinaryOperator ("{0} * {1}")]
        public static Mat3 operator * (Mat3 left, Mat3 right)
        {
			return new Mat3 (
				left.Column0.X * right.Column0.X + left.Column1.X * right.Column0.Y + left.Column2.X * right.Column0.Z,
				left.Column0.Y * right.Column0.X + left.Column1.Y * right.Column0.Y + left.Column2.Y * right.Column0.Z,
				left.Column0.Z * right.Column0.X + left.Column1.Z * right.Column0.Y + left.Column2.Z * right.Column0.Z,
				left.Column0.X * right.Column1.X + left.Column1.X * right.Column1.Y + left.Column2.X * right.Column1.Z,
				left.Column0.Y * right.Column1.X + left.Column1.Y * right.Column1.Y + left.Column2.Y * right.Column1.Z,
				left.Column0.Z * right.Column1.X + left.Column1.Z * right.Column1.Y + left.Column2.Z * right.Column1.Z,
				left.Column0.X * right.Column2.X + left.Column1.X * right.Column2.Y + left.Column2.X * right.Column2.Z,
				left.Column0.Y * right.Column2.X + left.Column1.Y * right.Column2.Y + left.Column2.Y * right.Column2.Z,
				left.Column0.Z * right.Column2.X + left.Column1.Z * right.Column2.Y + left.Column2.Z * right.Column2.Z);
        }

		/// <summary>
		/// Multiply a matrix by a Vec3 producing a vector with the same type.
		/// </summary>
		[GLBinaryOperator ("{0} * {1}")]
        public static Vec3 operator * (Mat3 mat, Vec3 vec)
        {
			return new Vec3 (
				mat.Column0.X * vec.X + mat.Column1.X * vec.Y + mat.Column2.X * vec.Z,
				mat.Column0.Y * vec.X + mat.Column1.Y * vec.Y + mat.Column2.Y * vec.Z,
				mat.Column0.Z * vec.X + mat.Column1.Z * vec.Y + mat.Column2.Z * vec.Z);
        }

		/// <summary>
		/// Divide a matrix by a scalar componentwise.
		/// </summary>
		[GLBinaryOperator ("{0} / {1}")]
        public static Mat3 operator / (Mat3 mat, float scalar)
        {
            return mat.Divide (scalar);
        }

		/// <summary>
		/// Add two matrices together componentwise.
		/// </summary>
		[GLBinaryOperator ("{0} + {1}")]
        public static Mat3 operator + (Mat3 left, Mat3 right)
        {
            return left.Add (right);
        }

		/// <summary>
		/// Overloaded equality operator that works with matrices.
		/// </summary>
        public static bool operator == (Mat3 left, Mat3 right)
        {
            return left.Equals (right);
        }

		/// <summary>
		/// Overloaded inequality operator that works with matrices.
		/// </summary>
        public static bool operator != (Mat3 left, Mat3 right)
        {
            return !left.Equals (right);
        }
	}
}
 
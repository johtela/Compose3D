namespace Compose3D.Maths
{
    using System;
	using System.Collections.Generic;
	using Extensions;
	using GLTypes;

	/// <summary>
	/// IVec{V, T} is a generic interface for vector types. It allows one to create generic 
	/// functions that are usable with all or some of the vector types. It also removes a lot of 
	/// repetitive code that would be otherwise needed to implement a same operation for different 
	/// `Vec*` structures.
	/// </summary>
	/// <typeparam name="V">The type of the vector. I.e. the type of the structure that implements
	/// this interface.</typeparam>
	/// <typeparam name="T">The type of the components in the vector.</typeparam>
	public interface IVec<V, T> : IEquatable<V>
		where V : struct, IVec<V, T>
		where T : struct, IEquatable<T>
	{
		/// <summary>
		/// Negate all of the components of the vector.
		/// </summary>
		V Invert ();

		/// <summary>
		/// Add another vector this one componentwise.
		/// </summary>
		V Add (V other);

		/// <summary>
		/// Subtract the given vector from this one componentwise.
		/// </summary>
		V Subtract (V other);

		/// <summary>
		/// Multiply the components of this vector with a same scalar value.
		/// </summary>
		V Multiply (T scalar);

		/// <summary>
		/// Multiply with another vector componentwise.
		/// </summary>
		V Multiply (V scale);

		/// <summary>
		/// Divide the components of this vector by a same scalar value.
		/// </summary>
		V Divide (T scalar);

		/// <summary>
		/// Divide by another vector componentwise.
		/// </summary>
		V Divide (V scale);

		/// <summary>
		/// Calculate the dot product with another vector.
		/// </summary>
		T Dot (V other);

		/// <summary>
		/// Number of dimensions/components in the vector.
		/// </summary>
		int Dimensions { get; }

		/// <summary>
		/// The value of the index'th component of the vector.
		/// </summary>
		T this[int index] { get; set; }

		/// <summary>
		/// The lengh of the vector.
		/// </summary>
		T Length { get; }

		/// <summary>
		/// The lengh of the vector squared. This is bit faster to calculate than the actual length
		/// because the square root operation is omitted.
		/// </summary>
		T LengthSquared { get; }

		/// <summary>
		/// The normalized vector. I.e. vector with same direction, but with lenght of 1.
		/// </summary>
		V Normalized { get; }
	}

	/// <summary>
	/// Collection of operations that operate on `Vec*` structures.
	/// </summary>
	/// Many of the GLSL functions that operate on vectors are contained in the `Vec` class.
	/// Most of these functions are implemented as extension methods, so that the operations
	/// are easy to discover from an IDE. They are of course transformed to regular functions
	/// in GLSL.
	/// 
	/// Some of the functions are only available in C#, in which case the method is *not*
	/// decorated by the <see cref="GLFunction"/> attribute. The method signatures are defined
	/// in terms of the most generic component type that can be used in the specific context. 
	/// Because there is no way in C# to treat numeric types like `float` or `int` generically, 
	/// there might be overloads for different primitive types. The only unifying property of 
	/// these types is that they all implement <see cref="IEquatable{T}"/> interface. This
	/// is used as the lowest common denominator when there is no need to refer to a particular
	/// arithmetic operation.
	public static class Vec
    {
		/// <summary>
		/// Create a vectory structure from an array.
		/// </summary>
		/// If you have an array of numbers, you can create a vector out of them generically
		/// using this function. This is handy in many cases when you want to convert vectors
		/// from one type to another or create them generically.
        public static V FromArray<V, T> (params T[] items)
            where V : struct, IVec<V, T>
            where T : struct, IEquatable<T>
        {
            var res = default (V);
            for (int i = 0; i < Math.Min (res.Dimensions, items.Length); i++)
                res[i] = items[i];
            return res;
        }

		/// <summary>
		/// Put the components of the vector to an array.
		/// </summary>
		/// This is the counteroperation of <see cref="FromArray{V, T}(T[])"/>. It takes a vector
		/// and returns its components in an array.
        public static T[] ToArray<V, T> (this V vec)
            where V : struct, IVec<V, T>
            where T : struct, IEquatable<T>
        {
            var res = new T[vec.Dimensions];
            for (int i = 0; i < vec.Dimensions; i++)
                res[i] = vec[i];
            return res;
        }

		public static V New<V, T> (T value)
			where V : struct, IVec<V, T>
			where T : struct, IEquatable<T>
		{
			var res = default (V);
			for (int i = 0; i < res.Dimensions; i++)
				res[i] = value;
			return res;
		}

		/// <summary>
		/// Convert a vector type to another vector type that has the same component type.
		/// </summary>
		/// This function is useful for generically transforming a vector to another vector
		/// type that has the same component type but different number of components.
		public static U Convert<V, U, T> (this V vec)
			where V : struct, IVec<V, T>
			where U : struct, IVec<U, T>
			where T : struct, IEquatable<T>
		{
			return FromArray<U, T> (vec.ToArray<V, T> ());
		}

		/// <summary>
		/// Returns true, when two vectors are approximetely same.
		/// </summary>
		/// Since rounding errors are quite common with `float`s that are used heavily in OpenGL,
		/// comparing for equality can be quite tricky. To alleviate the problem, this function
		/// compares the vectors approximately. The maximum allowed error is defined by the `epsilon`
		/// parameter. If you want to allow for error in 4th decimal, for example, you should pass
		/// in `epsilon` value of 0.001.
		public static bool ApproxEquals<V> (V vec, V other, float epsilon)
            where V : struct, IVec<V, float>
        {
            for (int i = 0; i < vec.Dimensions; i++)
				if (!vec[i].ApproxEquals (other[i], epsilon)) 
					return false;
            return true;
        }

		/// <summary>
		/// Calls <see cref="ApproxEquals{V}(V, V, float)"/> with an epsilon value of 0.000001.
		/// </summary>
		public static bool ApproxEquals<V> (V vec, V other)
			where V : struct, IVec<V, float>
		{
			return ApproxEquals (vec, other, 0.000001f);
		}

		/// <summary>
		/// Return a copy of a vector with one component changed.
		/// </summary>
		/// Many times it is necessary to change a single component of a vector. However, if you assign a
		/// value to the component directly, you will mutate the original vector. To quickly return a copy 
		/// of a vector with one component changed, this extension method is provided.
        public static V With<V, T> (this V vec, int i, T value)
            where V : struct, IVec<V, T>
            where T : struct, IEquatable<T>
        {
            var res = vec;
            res[i] = value;
            return res;
        }
		
		/// <summary>
		/// Return the sum of all the components in the vector.
		/// </summary>
		public static float Sum<V> (this V vec)
			where V : struct, IVec<V, float>
		{
			var res = vec [0];
			for (int i = 1; i < vec.Dimensions; i++)
				res += vec [i];
			return res;
		}

		/// <summary>
		/// Interpolate between two vectors. 
		/// </summary>
		/// Returns a sequence of vectors that contain the interpolated values. 
		/// The number of elemens in the sequence is given in the `step` parameter.
		public static IEnumerable<V> Interpolate<V> (this V from, V to, int steps)
			where V : struct, IVec<V, float>
		{
			var step = 1f / steps;
			var f = 0f;
			for (int i = 0; i < steps; i++, f += step)
				yield return from.Mix (to, f);
		}

		public static float DistanceTo<V> (this V from, V to)
			where V : struct, IVec<V, float>
		{
			return to.Subtract (from).Length;
		}

		public static float SquaredDistanceTo<V> (this V from, V to)
			where V : struct, IVec<V, float>
		{
			return to.Subtract (from).LengthSquared;
		}

		public static float ManhattanDistanceTo<V> (this V from, V to)
			where V : struct, IVec<V, float>
		{
			return to.Subtract (from).Abs ().Sum ();
		}

		/// <summary>
		/// Map the components of the vector to another vector of the same type.
		/// </summary>
		/// This function maps the components of a vector to another vector
		/// using a lambda exprerssion or function to do the transformation.
		public static V Map<V, T> (this V vec, Func<T, T> map)
			where V : struct, IVec<V, T>
			where T : struct, IEquatable<T>
		{
			var res = default (V);
			for (int i = 0; i < vec.Dimensions; i++)
				res[i] = map (vec[i]);
			return res;
		}

		/// <summary>
		/// Map the components of two vectors to another vector of the same type.
		/// </summary>
		/// This function maps the components of two vectors given as an argument to a result 
		/// vector using a lambda exprerssion or function to do the transformation.
		public static V Map2<V, T> (this V vec, V other, Func<T, T, T> map)
			where V : struct, IVec<V, T>
			where T : struct, IEquatable<T>
		{
			var res = default (V);
			for (int i = 0; i < vec.Dimensions; i++)
				res[i] = map (vec[i], other[i]);
			return res;
		}

		/// <summary>
		/// Map the components of three vectors to another vector of the same type.
		/// </summary>
		/// This function maps the components of three vectors given as an argument to a result 
		/// vector using a lambda exprerssion or function to do the transformation.
		public static V Map3<V, T> (this V vec1, V vec2, V vec3, Func<T, T, T, T> map)
			where V : struct, IVec<V, T>
			where T : struct, IEquatable<T>
		{
			var res = default (V);
			for (int i = 0; i < vec1.Dimensions; i++)
				res[i] = map (vec1[i], vec2[i], vec3[i]);
			return res;
		}

		public static bool All<V, T> (this V vec, Func<T, bool> predicate)
			where V : struct, IVec<V, T>
			where T : struct, IEquatable<T>
		{
			for (int i = 0; i < vec.Dimensions; i++)
				if (!predicate (vec[i]))
					return false;
			return true;
		}

		public static bool Any<V, T> (this V vec, Func<T, bool> predicate)
			where V : struct, IVec<V, T>
			where T : struct, IEquatable<T>
		{
			for (int i = 0; i < vec.Dimensions; i++)
				if (predicate (vec[i]))
					return true;
			return false;
		}

		/// <summary>
		/// Calculate the normal vector given three points in a plane using the vector cross product.
		/// </summary>
		/// All of the points need to be unique, otherwise the calculation does not work. The direction
		/// of the normal depends on the order in which the positions are given. If you find that the 
		/// normal is pointing to an opposite direction, switch the order of `adjecentPos1` and 
		/// `adjacentPos2` parameters.
		public static Vec3 CalculateNormal (this Vec3 position, Vec3 adjacentPos1, Vec3 adjacentPos2)
		{
			return (adjacentPos1 - position).Cross (adjacentPos2 - position).Normalized;
		}

		public static bool AreCollinear (this Vec3 pos1, Vec3 pos2, Vec3 pos3)
		{
			var vec1 = (pos2 - pos1).Normalized;
			var vec2 = (pos3 - pos1).Normalized;
			return vec1 == vec2 || vec1 == -vec2;
		}

		/// <summary>
		/// The cross product of two vectors.
		/// </summary>
		/// Cross returns a vector perpendicular to the two vectors given as arguments. This operation
		/// only makes sense in 3D, so function is only defined for <see cref="Vec3"/>.
		[GLFunction ("cross ({0})")]
		public static Vec3 Cross (this Vec3 v1, Vec3 v2)
		{
			return new Vec3 (
				v1.Y * v2.Z - v1.Z * v2.Y,
				v1.Z * v2.X - v1.X * v2.Z,
				v1.X * v2.Y - v1.Y * v2.X);			
		}

		/// <summary>
		/// The angle of the vector in YZ-plane. I.e. the angle of rotation around X-axis.
		/// </summary>
		public static float XRotation (this Vec3 vec)
		{
			return GLMath.Atan2 (-vec.Y, vec.Z);
		}

		/// <summary>
		/// The angle of the vector in XZ-plane. I.e. the angle of rotation around Y-axis.
		/// </summary>
		public static float YRotation (this Vec3 vec)
		{
			return GLMath.Atan2 (vec.X, vec.Z);
		}

		/// <summary>
		/// Convert a vector of degree values to radians.
		/// </summary>
		[GLFunction ("radians ({0})")]
		public static V Radians<V> (this V vec)
			where V : struct, IVec<V, float>
		{
			return vec.Map<V, float> (GLMath.Radians);
		}

		/// <summary>
		/// Convert a vector of radian values to degrees.
		/// </summary>
		[GLFunction ("degrees ({0})")]
		public static V Degrees<V> (this V vec)
			where V : struct, IVec<V, float>
		{
			return vec.Map<V, float> (GLMath.Degrees);
		}

		/// <summary>
		/// Applies the <see cref="Math.Abs(float)"/> function to the vector componentwise.
		/// </summary>
		[GLFunction ("abs ({0})")]
		public static V Abs<V> (this V vec)
			where V : struct, IVec<V, float>
		{
			return vec.Map<V, float> (Math.Abs);
		}

		/// <summary>
		/// Applies the <see cref="GLMath.Floor(float)"/> function to the vector componentwise.
		/// </summary>
		[GLFunction ("floor ({0})")]
		public static V Floor<V> (this V vec)
			where V : struct, IVec<V, float>
		{
			return vec.Map<V, float> (GLMath.Floor);
		}

		/// <summary>
		/// Applies the <see cref="GLMath.Ceiling(float)"/> function to the vector componentwise.
		/// </summary>
		[GLFunction ("ceil ({0})")]
		public static V Ceiling<V> (this V vec)
			where V : struct, IVec<V, float>
		{
			return vec.Map<V, float> (GLMath.Ceiling);
		}

		/// <summary>
		/// Applies the <see cref="GLMath.Truncate(float)"/> function to the vector componentwise.
		/// </summary>
		[GLFunction ("trunc ({0})")]
		public static V Truncate<V> (this V vec)
			where V : struct, IVec<V, float>
		{
			return vec.Map<V, float> (GLMath.Truncate);
		}

		/// <summary>
		/// Applies the <see cref="GLMath.Fraction(float)"/> function to the vector componentwise.
		/// </summary>
		[GLFunction ("fract ({0})")]
		public static V Fraction<V> (this V vec)
			where V : struct, IVec<V, float>
		{
			return vec.Map<V, float> (GLMath.Fraction);
		}

		/// <summary>
		/// Applies the <see cref="Math.Min(float, float)"/> function to the vector componentwise.
		/// </summary>
		[GLFunction ("min ({0})")]
		public static V Min<V> (this V vec, V other)
			where V : struct, IVec<V, float>
		{
			return vec.Map2<V, float> (other, Math.Min);
		}

		/// <summary>
		/// Applies the <see cref="Math.Min(int, int)"/> function to the vector componentwise.
		/// </summary>
		[GLFunction ("min ({0})")]
		public static V Mini<V> (this V vec, V other)
			where V : struct, IVec<V, int>
		{
			return vec.Map2<V, int> (other, Math.Min);
		}

		/// <summary>
		/// Applies the <see cref="Math.Max(float, float)"/> function to the vector componentwise.
		/// </summary>
		[GLFunction ("max ({0})")]
		public static V Max<V> (this V vec, V other)
			where V : struct, IVec<V, float>
		{
			return vec.Map2<V, float> (other, Math.Max);
		}

		/// <summary>
		/// Applies the <see cref="Math.Max(int, int)"/> function to the vector componentwise.
		/// </summary>
		[GLFunction ("max ({0})")]
		public static V Maxi<V> (this V vec, V other)
			where V : struct, IVec<V, int>
		{
			return vec.Map2<V, int> (other, Math.Max);
		}

		/// <summary>
		/// Applies the <see cref="GLMath.Clamp(float, float, float)"/> function to the vector componentwise.
		/// </summary>
		[GLFunction ("clamp ({0})")]
		public static V Clamp<V> (this V vec, float min, float max)
			where V : struct, IVec<V, float>
		{
			return vec.Map<V, float> (a => GLMath.Clamp (a, min, max));
		}

		/// <summary>
		/// Applies the <see cref="GLMath.Clamp(int, int, int)"/> function to the vector componentwise.
		/// </summary>
		[GLFunction ("clamp ({0})")]
		public static V Clamp<V> (this V vec, int min, int max)
			where V : struct, IVec<V, int>
		{
			return vec.Map<V, int> (a => GLMath.Clamp (a, min, max));
		}

		/// <summary>
		/// Applies the <see cref="GLMath.Clamp(float, float, float)"/> function to the vector componentwise.
		/// The minimum and maximum values are also given as vectors.
		/// </summary>
		[GLFunction ("clamp ({0})")]
		public static V Clamp<V> (this V vec, V min, V max)
			where V : struct, IVec<V, float>
		{
			return vec.Map3<V, float> (min, max, GLMath.Clamp);
		}

		/// <summary>
		/// Applies the <see cref="GLMath.Clamp(int, int, int)"/> function to the vector componentwise.
		/// The minimum and maximum values are also given as vectors.
		/// </summary>
		[GLFunction ("clamp ({0})")]
		public static V Clampi<V> (this V vec, V min, V max)
			where V : struct, IVec<V, int>
		{
			return vec.Map3<V, int> (min, max, GLMath.Clamp);
		}

		/// <summary>
		/// Applies the <see cref="GLMath.Mix(float, float, float)"/> function to the vector componentwise.
		/// The interPos parameter is also given as vector.
		/// </summary>
		[GLFunction ("mix ({0})")]
		public static V Mix<V> (this V vec, V other, V interPos)
			where V : struct, IVec<V, float>
		{
			return vec.Map3<V, float> (other, interPos, GLMath.Mix);
		}

		/// <summary>
		/// Applies the <see cref="GLMath.Mix(float, float, float)"/> function to the vector componentwise.
		/// </summary>
		[GLFunction ("mix ({0})")]
		public static V Mix<V> (this V vec, V other, float interPos)
			where V : struct, IVec<V, float>
		{
			return vec.Map2<V, float> (other, (x, y) => GLMath.Mix (x, y, interPos));
		}

		/// <summary>
		/// Applies the <see cref="GLMath.Step(float, float)"/> function to the vector componentwise.
		/// </summary>
		[GLFunction ("step ({0})")]
		public static V Step<V> (float edge, V vec)
			where V : struct, IVec<V, float>
		{
			return vec.Map<V, float> (a => GLMath.Step (edge, a));
		}

		/// <summary>
		/// Applies the <see cref="GLMath.Step(float, float)"/> function to the vector componentwise.
		/// The edge values are also given as vector.
		/// </summary>
		[GLFunction ("step ({0})")]
		public static V Step<V> (V edge, V vec)
			where V : struct, IVec<V, float>
		{
			return edge.Map2<V, float> (vec, GLMath.Step);
		}

		/// <summary>
		/// Applies the <see cref="GLMath.SmoothStep(float, float, float)"/> function to the vector componentwise.
		/// </summary>
		[GLFunction ("smoothstep ({0})")]
		public static V SmoothStep<V> (float edgeLower, float edgeUpper, V vec)
			where V : struct, IVec<V, float>
		{
			return vec.Map<V, float> (a => GLMath.SmoothStep (edgeLower, edgeUpper, a));
		}

		/// <summary>
		/// Applies the <see cref="GLMath.SmoothStep(float, float, float)"/> function to the vector componentwise.
		/// The edge values are also given as vector.
		/// </summary>
		[GLFunction ("smoothstep ({0})")]
		public static V SmoothStep<V> (V edgeLower, V edgeUpper, V vec)
			where V : struct, IVec<V, float>
		{
			return edgeLower.Map3<V, float> (edgeUpper, vec, GLMath.SmoothStep);
		}

		/// <summary>
		/// Applies the <see cref="GLMath.Pow(float, float)"/> function to the vector componentwise.
		/// The exp values are also given as vector.
		/// </summary>
		[GLFunction ("pow ({0})")]
		public static V Pow<V> (this V vec, V exp)
			where V : struct, IVec<V, float>
		{
			return vec.Map2<V, float> (exp, GLMath.Pow);
		}

		/// <summary>
		/// Applies the <see cref="GLMath.Exp(float)"/> function to the vector componentwise.
		/// </summary>
		[GLFunction ("exp ({0})")]
		public static V Exp<V> (this V vec)
			where V : struct, IVec<V, float>
		{
			return vec.Map<V, float> (GLMath.Exp);
		}

		/// <summary>
		/// Applies the <see cref="GLMath.Log(float)"/> function to the vector componentwise.
		/// </summary>
		[GLFunction ("log ({0})")]
		public static V Log<V> (this V vec)
			where V : struct, IVec<V, float>
		{
			return vec.Map<V, float> (GLMath.Log);
		}

		/// <summary>
		/// Applies the <see cref="GLMath.Sqrt(float)"/> function to the vector componentwise.
		/// </summary>
		[GLFunction ("sqrt ({0})")]
		public static V Sqrt<V> (this V vec)
			where V : struct, IVec<V, float>
		{
			return vec.Map<V, float> (GLMath.Sqrt);
		}

		/// <summary>
		/// Applies the <see cref="GLMath.InverseSqrt(float)"/> function to the vector componentwise.
		/// </summary>
		[GLFunction ("inversesqrt ({0})")]
		public static V InverseSqrt<V> (this V vec)
			where V : struct, IVec<V, float>
		{
			return vec.Map<V, float> (GLMath.InverseSqrt);
		}

		/// <summary>
		/// Calculates the reflection vector along given normal.
		/// </summary>
		/// The reflect function returns a vector that points in the direction of reflection.
		/// The function has two input parameters: the incident vector, and the normal vector 
		/// of the reflecting surface.
		/// 
		/// Note: To obtain the desired result the `along` vector has to be normalized. The reflection 
		/// vector always has the same length as the incident vector. From this it follows that the 
		/// reflection vector is normalized if `vec` and `along` vectors are both normalized.		
		[GLFunction ("reflect ({0})")]
		public static V Reflect<V, T> (this V vec, V along)
			where V : struct, IVec<V, float>
			where T : struct, IEquatable<T>
		{
			return vec.Subtract (along.Multiply (2 * vec.Dot (along)));
		}

		/// <summary>
		/// Applies the <see cref="GLMath.Sin(float)"/> function to the vector componentwise.
		/// </summary>
		[GLFunction ("sin ({0})")]
		public static V Sin<V> (this V angles)
			where V : struct, IVec<V, float>
		{
			return angles.Map<V, float> (GLMath.Sin);
		}

		/// <summary>
		/// Applies the <see cref="GLMath.Cos(float)"/> function to the vector componentwise.
		/// </summary>
		[GLFunction ("cos ({0})")]
		public static V Cos<V> (this V angles)
			where V : struct, IVec<V, float>
		{
			return angles.Map<V, float> (GLMath.Cos);
		}

		/// <summary>
		/// Applies the <see cref="GLMath.Tan(float)"/> function to the vector componentwise.
		/// </summary>
		[GLFunction ("tan ({0})")]
		public static V Tan<V> (this V angles)
			where V : struct, IVec<V, float>
		{
			return angles.Map<V, float> (GLMath.Tan);
		}

		/// <summary>
		/// Applies the <see cref="GLMath.Asin(float)"/> function to the vector componentwise.
		/// </summary>
		[GLFunction ("asin ({0})")]
		public static V Asin<V> (this V x)
			where V : struct, IVec<V, float>
		{
			return x.Map<V, float> (GLMath.Asin);
		}

		/// <summary>
		/// Applies the <see cref="GLMath.Acos(float)"/> function to the vector componentwise.
		/// </summary>
		[GLFunction ("acos ({0})")]
		public static V Acos<V> (this V x)
			where V : struct, IVec<V, float>
		{
			return x.Map<V, float> (GLMath.Acos);
		}

		/// <summary>
		/// Applies the <see cref="GLMath.Atan(float)"/> function to the vector componentwise.
		/// </summary>
		[GLFunction ("atan ({0})")]
		public static V Atan<V> (this V y_over_x)
			where V : struct, IVec<V, float>
		{
			return y_over_x.Map<V, float> (GLMath.Atan);
		}

		/// <summary>
		/// Applies the <see cref="GLMath.Atan2(float, float)"/> function to the vector componentwise.
		/// </summary>
		[GLFunction ("atan ({0})")]
		public static V Atan2<V> (this V y, V x)
			where V : struct, IVec<V, float>
		{
			return y.Map2<V, float> (x, GLMath.Atan2);
		}

		/// <summary>
		/// Check whether any of the components of the vector are NaN. 
		/// </summary>
		public static bool IsNaN<V> (this V vec)
			where V : struct, IVec<V, float>
		{
			return vec.Any<V, float> (float.IsNaN);
		}

		private static Random _random = new Random ();

		public static V Random<V> (float range)
			where V : struct, IVec<V, float>
		{
			return Random<V> (_random, range);
		}

		public static V Random<V> (Random rnd, float range)
			where V : struct, IVec<V, float>
		{
			var offs = range / 2f;
			return FromArray<V, float> (
				(float)rnd.NextDouble () * range - offs,
				(float)rnd.NextDouble () * range - offs,
				(float)rnd.NextDouble () * range - offs,
				(float)rnd.NextDouble () * range - offs);
		}
	}
}
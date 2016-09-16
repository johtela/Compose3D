namespace Compose3D.Maths
{
	using GLTypes;
	using OpenTK;
	using System;

	/// <summary>
	/// Extension class that contains mathematical functions supported by GLSL.
	/// </summary>
	/// A lot of the math functions supported by GLSL can be found in the <see cref="Math"/> class, 
	/// but it only has the versions with <see cref="double"/> as parameters. To make all, or at least
	/// most of the GLSL functions available in C#, the <see cref="float"/> versions are included here.
	/// 
	/// There are also quite commonly needed functions like <see cref="Clamp(double, double, double)"/> 
	/// or <see cref="Mix(double, double, double)"/> that do not exist in the System.Math class.
	/// 
	/// The vector overloads of these functions can be found in the <see cref="Vec"/> class.
	public static class GLMath
	{
		/// <summary>
		/// Returns the largest integer number that is smaller or equal to `value`.
		/// </summary>
		[GLFunction ("floor ({0})")]
		public static float Floor (this float value)
		{
			return (float)Math.Floor (value);
		}

		/// <summary>
		/// Returns the smallest number that is larger or equal to `value`.
		/// </summary>
		[GLFunction ("ceil ({0})")]
		public static float Ceiling (this float value)
		{
			return (float)Math.Ceiling (value);
		}

		/// <summary>
		/// Returns a a value equal to the nearest integer to the `value` parameter whose absolute 
		/// value is not larger than the absolute value of the parameter. 
		/// </summary>
		/// In Stack Overflow there is a better explanation about the difference of this <see cref="Floor(float)"/>,
		/// and <see cref="Ceiling(float)"/> function:
		/// 
		/// >	Floor rounds down, Ceiling rounds up, and Truncate rounds towards zero. Thus, Truncate is Floor for 
		/// >	positive numbers, and like Ceiling for negative numbers.
		[GLFunction ("trunc ({0})")]
		public static float Truncate (this float value)
		{
			return (float)Math.Truncate (value);
		}

		/// <summary>
		/// Returns the fractional part of `value`. This is calculated as `value - floor(value)`.
		/// </summary>
		[GLFunction ("fract ({0})")]
		public static float Fraction (this float value)
		{
			return value - Floor (value);
		}

		/// <summary>
		/// Returns the fractional part of `value`. This is calculated as `value - floor(value)`.
		/// </summary>
		[GLFunction ("fract ({0})")]
		public static double Fraction (this double value)
		{
			return value - Math.Floor (value);
		}

		/// <summary>
		/// Returns the `value` parameter unchanged if it is larger than `min` and smaller than `max`. 
		/// In case `value` is smaller than the `min` parameter, then `min` is returned. 
		/// If `value` is larger than the `max` parameter, then that is returned.
		/// </summary>
		[GLFunction ("clamp ({0})")]
		public static float Clamp (this float value, float min, float max)
		{
			return 
				value < min ? min :
				value > max ? max :
				value;
		}

		/// <summary>
		/// Returns the `value` parameter unchanged if it is larger than `min` and smaller than `max`. 
		/// In case `value` is smaller than the `min` parameter, then `min` is returned. 
		/// If `value` is larger than the `max` parameter, then that is returned.
		/// </summary>
		[GLFunction ("clamp ({0})")]
		public static double Clamp (this double value, double min, double max)
		{
			return 
				value < min ? min :
				value > max ? max :
				value;
		}

		/// <summary>
		/// Returns the `value` parameter unchanged if it is larger than `min` and smaller than `max`. 
		/// In case `value` is smaller than the `min` parameter, then `min` is returned. 
		/// If `value` is larger than the `max` parameter, then that is returned.
		/// </summary>
		[GLFunction ("clamp ({0})")]
		public static int Clamp (this int value, int min, int max)
		{
			return Math.Min (Math.Max (value, min), max);
		}

		/// <summary>
		/// Returns the linear blend (interpolation) between `start` and `end` parameters. 
		/// I.e. the product of `start` and `(1 - interPos)` plus the product of `end` and `interPos`.
		/// </summary>
		[GLFunction ("mix ({0})")]
		public static float Mix (float start, float end, float interPos)
		{
			return start + (interPos * (start - end));
		}

		/// <summary>
		/// Returns the linear blend (interpolation) between `start` and `end` parameters. 
		/// I.e. the product of `start` and `(1 - interPos)` plus the product of `end` and `interPos`.
		/// </summary>
		[GLFunction ("mix ({0})")]
		public static double Mix (double start, double end, double interPos)
		{
			return start + (interPos * (start - end));
		}

		/// <summary>
		/// Returns the cosine interpolation between `start` and `end` parameters. 
		/// </summary>
		/// This function differs from the linear <see cref="Mix(float, float, float)"/> interpolatoin 
		/// in how the intermediate values or calculated. Instead of following a linear path between 
		/// the values, the path follows a cosine curve. This produces a smoother  than linear interpolation.
		/// 
		/// *Note:* This function is not available in GLSL. It is included here for convenience.
		public static float CosMix (float start, float end, float interPos)
		{
			return Mix (start, end, (1f - Cos (interPos * MathHelper.Pi)) * 0.5f); 
		}

		/// <summary>
		/// Returns 0.0 if `value` is smaller then `edge` and otherwise 1.0.
		/// </summary>
		[GLFunction ("step ({0})")]
		public static float Step (float edge, float value)
		{
			return value < edge ? 0f : 1f;
		}

		/// <summary>
		/// Returns 0.0 if `value` is smaller then `edge` and otherwise 1.0.
		/// </summary>
		[GLFunction ("step ({0})")]
		public static double Step (double edge, double value)
		{
			return value < edge ? 0.0 : 1.0;
		}

		/// <summary>
		/// Returns 0.0 if `value` is smaller then `edgeLower` and 1.0 if `value` is larger than `edgeUpper`. 
		/// Otherwise the return value is interpolated between 0.0 and 1.0 using Hermite polynomials. 
		/// </summary>
		[GLFunction ("smoothstep ({0})")]
		public static float SmoothStep (float edgeLower, float edgeUpper, float value)
		{
			var t = Clamp ((value - edgeLower) / (edgeUpper - edgeLower), 0f, 1f);
			return t * t * (3f - (2f * t));
		}

		/// <summary>
		/// Returns 0.0 if `value` is smaller then `edgeLower` and 1.0 if `value` is larger than `edgeUpper`. 
		/// Otherwise the return value is interpolated between 0.0 and 1.0 using Hermite polynomials. 
		/// </summary>
		[GLFunction ("smoothstep ({0})")]
		public static double SmoothStep (double edgeLower, double edgeUpper, double value)
		{
			var t = Clamp ((value - edgeLower) / (edgeUpper - edgeLower), 0.0, 1.1);
			return t * t * (3.0 - (2.0 * t));
		}

		/// <summary>
		/// Returns `value` raised to the power of `exponent`.
		/// </summary>
		[GLFunction ("pow ({0})")]
		public static float Pow (this float value, float exponent)
		{
			return (float)Math.Pow (value, exponent);
		}

		/// <summary>
		/// Returns `value` raised to the power of `exponent`.
		/// </summary>
		[GLFunction ("pow ({0})")]
		public static int Pow (this int value, int exponent)
		{
			return (int)Math.Pow (value, exponent);
		}

		/// <summary>
		/// Returns the constant e raised to the power of `value`.
		/// </summary>
		[GLFunction ("exp ({0})")]
		public static float Exp (this float value)
		{
			return (float)Math.Exp (value);
		}

		/// <summary>
		/// Returns the power to which the constant e has to be raised to produce `value`.
		/// </summary>
		[GLFunction ("log ({0})")]
		public static float Log (this float value)
		{
			return (float)Math.Log (value);
		}

		/// <summary>
		/// Returns the square root of `value`.
		/// </summary>
		[GLFunction ("sqrt ({0})")]
		public static float Sqrt (this float value)
		{
			return (float)Math.Sqrt (value);
		}

		/// <summary>
		/// Returns the inverse of square root of `value`. I.e. `1 / sqrt (value)`.
		/// </summary>
		[GLFunction ("inversesqrt ({0})")]
		public static float InverseSqrt (this float value)
		{
			return 1f / (float)Math.Sqrt (value);
		}

		/// <summary>
		/// Converts degrees to radians.
		/// </summary>
		[GLFunction ("radians ({0})")]
		public static float Radians (this float degrees)
		{
			return degrees * MathHelper.Pi / 180f;
		}

		/// <summary>
		/// Converts degrees to radians.
		/// </summary>
		[GLFunction ("radians ({0})")]
		public static double Radians (this double degrees)
		{
			return degrees * Math.PI / 180.0;
		}

		/// <summary>
		/// Converts degrees to radians.
		/// </summary>
		public static float Radians (this int degrees)
		{
			return degrees * MathHelper.Pi / 180f;
		}

		/// <summary>
		/// Convert radians to degrees.
		/// </summary>
		[GLFunction ("radians ({0})")]
		public static float Degrees (this float radians)
		{
			return radians * 180f / MathHelper.Pi;
		}

		/// <summary>
		/// Convert radians to degrees.
		/// </summary>
		[GLFunction ("radians ({0})")]
		public static double Degrees (this double radians)
		{
			return radians * 180f / MathHelper.Pi;
		}

		/// <summary>
		/// Returns the sine of an `angle` in radians.
		/// </summary>
		[GLFunction ("sin ({0})")]
		public static float Sin (this float angle)
		{
			return (float)Math.Sin (angle);
		}

		/// <summary>
		/// Returns the cosine of an `angle` in radians.
		/// </summary>
		[GLFunction ("cos ({0})")]
		public static float Cos (this float angle)
		{
			return (float)Math.Cos (angle);
		}

		/// <summary>
		/// Returns the tangent of an `angle` in radians.
		/// </summary>
		[GLFunction ("tan ({0})")]
		public static float Tan (this float angle)
		{
			return (float)Math.Tan (angle);
		}

		/// <summary>
		/// Returns the arcsine of an `angle` in radians.
		/// </summary>
		[GLFunction ("asin ({0})")]
		public static float Asin (this float x)
		{
			return (float)Math.Asin (x);
		}

		/// <summary>
		/// Returns the arccosine of an `angle` in radians.
		/// </summary>
		[GLFunction ("acos ({0})")]
		public static float Acos (this float x)
		{
			return (float)Math.Acos (x);
		}

		/// <summary>
		/// Returns the arctangent of an `y_over_x` in radians.
		/// </summary>
		[GLFunction ("atan ({0})")]
		public static float Atan (this float y_over_x)
		{
			return (float)Math.Atan (y_over_x);
		}

		/// <summary>
		/// For a point with Cartesian coordinates `(x, y)` the function returns the 
		/// angle θ of the same point with polar coordinates (r, θ).
		/// </summary>
		[GLFunction ("atan ({0})")]
		public static float Atan2 (float y, float x)
		{
			return (float)Math.Atan2 (y, x);
		}

		/// <summary>
		/// Returns the absolute value of `value`. I.e. `value` when it is positive or zero and 
		/// `-value` when it is negative.
		/// </summary>
		[GLFunction ("abs ({0})")]
		public static float Abs (this float value)
		{
			return (float)Math.Abs (value);
		}

		/// <summary>
		/// Available only in the fragment shader, the function returns the partial derivative of `value` with 
		/// respect to the window x coordinate. Not implmented in C#.
		/// </summary>
		[GLFunction ("dFdx ({0})")]
		public static float dFdx (this float value)
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Available only in the fragment shader, the function returns the partial derivative of `value` with 
		/// respect to the window y coordinate. Not implmented in C#.
		/// </summary>
		[GLFunction ("dFdy ({0})")]
		public static float dFdy (this float value)
		{
			throw new NotImplementedException ();
		}
	}
}
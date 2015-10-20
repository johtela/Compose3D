namespace Compose3D.Arithmetics
{
	using GLTypes;
	using OpenTK;
	using System;

	public static class GLMath
	{
		[GLFunction ("floor ({0})")]
		public static float Floor (this float value)
		{
			return (float)Math.Floor (value);
		}

		[GLFunction ("ceil ({0})")]
		public static float Ceiling (this float value)
		{
			return (float)Math.Ceiling (value);
		}

		[GLFunction ("clamp ({0})")]
		public static float Clamp (this float value, float min, float max)
		{
			return Math.Min (Math.Max (value, min), max);
		}

		[GLFunction ("clamp ({0})")]
		public static double Clamp (this double value, double min, double max)
		{
			return Math.Min (Math.Max (value, min), max);
		}

		[GLFunction ("clamp ({0})")]
		public static int Clamp (this int value, int min, int max)
		{
			return Math.Min (Math.Max (value, min), max);
		}

		[GLFunction ("mix ({0})")]
		public static float Mix (float start, float end, float interPos)
		{
			return start * (1f - interPos) + end * interPos;
		}

		[GLFunction ("mix ({0})")]
		public static double Mix (double start, double end, double interPos)
		{
			return start * (1.0 - interPos) + end * interPos;
		}

		[GLFunction ("step ({0})")]
		public static float Step (float edge, float value)
		{
			return value < edge ? 0f : 1f;
		}

		[GLFunction ("step ({0})")]
		public static double Step (double edge, double value)
		{
			return value < edge ? 0.0 : 1.0;
		}

		[GLFunction ("smoothstep ({0})")]
		public static float SmoothStep (float edgeLower, float edgeUpper, float value)
		{
			var t = Clamp ((value - edgeLower) / (edgeUpper - edgeLower), 0f, 1f);
			return t * t * (3f - (2f * t));
		}

		[GLFunction ("smoothstep ({0})")]
		public static double SmoothStep (double edgeLower, double edgeUpper, double value)
		{
			var t = Clamp ((value - edgeLower) / (edgeUpper - edgeLower), 0.0, 1.1);
			return t * t * (3.0 - (2.0 * t));
		}

		[GLFunction ("pow ({0})")]
		public static float Pow (this float value, float exponent)
		{
			return (float)Math.Pow (value, exponent);
		}

		[GLFunction ("pow ({0})")]
		public static int Pow (this int value, int exponent)
		{
			return (int)Math.Pow (value, exponent);
		}

		[GLFunction ("exp ({0})")]
		public static float Exp (this float value)
		{
			return (float)Math.Exp (value);
		}

		[GLFunction ("log ({0})")]
		public static float Log (this float value)
		{
			return (float)Math.Log (value);
		}

		[GLFunction ("sqrt ({0})")]
		public static float Sqrt (this float value)
		{
			return (float)Math.Sqrt (value);
		}

		[GLFunction ("sqrt ({0})")]
		public static float InverseSqrt (this float value)
		{
			return 1f / (float)Math.Sqrt (value);
		}

		[GLFunction ("radians ({0})")]
		public static float Radians (this float degrees)
		{
			return degrees * MathHelper.Pi / 180f;
		}

		[GLFunction ("radians ({0})")]
		public static double Radians (this double degrees)
		{
			return degrees * Math.PI / 180.0;
		}

		public static float Radians (this int degrees)
		{
			return degrees * MathHelper.Pi / 180f;
		}

		[GLFunction ("radians ({0})")]
		public static float Degrees (this float radians)
		{
			return radians * 180f / MathHelper.Pi;
		}

		[GLFunction ("radians ({0})")]
		public static double Degrees (this double radians)
		{
			return radians * 180f / MathHelper.Pi;
		}

		[GLFunction ("sin ({0})")]
		public static float Sin (float angle)
		{
			return (float)Math.Sin (angle);
		}

		[GLFunction ("cos ({0})")]
		public static float Cos (float angle)
		{
			return (float)Math.Cos (angle);
		}

		[GLFunction ("tan ({0})")]
		public static float Tan (float angle)
		{
			return (float)Math.Tan (angle);
		}

		[GLFunction ("asin ({0})")]
		public static float Asin (float x)
		{
			return (float)Math.Asin (x);
		}

		[GLFunction ("acos ({0})")]
		public static float Acos (float x)
		{
			return (float)Math.Acos (x);
		}

		[GLFunction ("atan ({0})")]
		public static float Atan (float y_over_x)
		{
			return (float)Math.Atan (y_over_x);
		}

		[GLFunction ("atan ({0})")]
		public static float Atan2 (float y, float x)
		{
			return (float)Math.Atan2 (y, x);
		}
	}
}

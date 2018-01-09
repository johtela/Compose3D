/*
# Extensions for Number Types

Most of the standard methods manipulating number types (`int`, `float`, 
`double`, and so on) live in the System.Math class. Here we define few
operations and shortcuts which are missing from Math. Since the Math
class was developed before the extension methods were supported in C#,
all its methods are regular static methods. We, however, are exploiting
the extension methods wherever possible
*/
namespace Extensions
{
	using System;
	using System.Linq;

	public static class NumExt
	{
		/*
		## Number Inside a Range

		To check whether a number is between some minimum and maximum value,
		we define the following method.
		*/
		public static bool IsBetween (this int number, int floor, int ceil) => 
			number >= floor && number <= ceil;
		/*
		## Approximate Equivalence

		The `==` operator returns true for two floating point numbers only if
		they are exactly the same. Usually this is too strict, since floating 
		point types, especially `float`, is notorius for its rounding errors.
		In most cases, it is enough if the two values compared are close enough,
		or within a specified error margin.

		The methods below compare two floats or doubles and return true, if 
		their absolute difference is less than the epsilon parameter. A good
		value for the epsilon is found easiest by experimenting. The rounding
		errors accumulate, so depending on the scenario a wider margin might
		be necessary.
		*/
		public static bool ApproxEquals (this float x, float y, 
			float epsilon = 1e-06f)
		{
			if (x == y)
				return true;

			float absX = Math.Abs (x);
			float absY = Math.Abs (y);
			float diff = Math.Abs (x - y);

			if (x * y == 0)
				return diff < (epsilon * epsilon);
			else
				return diff / (absX + absY) < epsilon;
		}

		public static bool ApproxEquals (this double x, double y,
			double epsilon = 1e-11)
		{
			if (x == y)
				return true;

			double absX = Math.Abs (x);
			double absY = Math.Abs (y);
			double diff = Math.Abs (x - y);

			if (x * y == 0)
				return diff < (epsilon * epsilon);
			else
				return diff / (absX + absY) < epsilon;
		}
		/*
		The generic version of `ApproxEquals` accepts any value type, but throws
		an exception if the type is not `float` or `double`.
		*/
		public static bool ApproxEquals<T> (this T x, T y)
			where T : struct, IEquatable<T>
		{
			if (typeof (T) == typeof (float))
				return ApproxEquals ((float)((object)x), (float)((object)y), 1e-06f);
			else if (typeof (T) == typeof (double))
				return ApproxEquals ((double)((object)x), (double)((object)y), 1e-11);
			else
				throw new ArgumentException (
					"This method is only defined for floats and doubles.");
		}
		/*
		## Minimum and Maximum of Multiple Values

		The Min and Max methods in the System.Math class accept only two paremeters.
		When you want to find the min/max of multiple values, you can use the methods
		below. Instead of overloading, theit genericity is achieved through the 
		`IComparable` interface.
		*/
		public static T Min<T> (params T[] values) where T : struct, IComparable<T>
		{
			return values.Min ();
		}

		public static T Max<T> (params T[] values) where T : struct, IComparable<T>
		{
			return values.Max ();
		}
		/*
		## Count Number of Bits Set

		If you need to know the number of 1 bits in an integer, you can get it
		with the method below.
		*/
		public static int NumberOfBitsSet (this int x)
		{
			var result = 0;
			for (int i = 0; i < 32; i++)
			{
				if ((x & 1) == 1)
					result++;
				x >>= 1;
			}
			return result;
		}
	}
}

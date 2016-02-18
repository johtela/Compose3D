namespace Extensions
{
	using System;
	using System.Linq;

	public static class NumExt
	{
		public static bool IsBetween (this int number, int floor, int ceil)
		{
			return number >= floor && number <= ceil;
		}

		public static bool ApproxEquals (this float x, float y, float epsilon)
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

		public static bool ApproxEquals (this float x, float y)
		{
			return x.ApproxEquals (y, 0.000001f);
		}

		public static bool ApproxEquals<T> (this T x, T y)
			where T : struct, IEquatable<T>
		{
			if (typeof (T) == typeof (float))
				return ApproxEquals ((float)((object)x), (float)((object)y));
			else
				throw new ArgumentException ("This method is only defined for floats.");
		}

		public static T Max<T> (params T[] values) where T : struct, IComparable<T>
		{
			return values.Max ();
		}
	}
}

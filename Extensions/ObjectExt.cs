namespace Extensions
{
	using System;
	using System.Linq;

	public static class ObjectExt
	{
		public static bool In<T> (this T obj, params T[] alternatives)
		{
			return alternatives.Contains (obj);
		}

		public static bool IsDefault<T> (this T obj)
		{
			return obj.Equals (default (T));
		}

		public static bool NotDefault<T> (this T obj)
		{
			return !obj.Equals (default (T));
		}

		public static S Match<T, S> (this object expr, Func<T, S> func)
			where T : class
			where S : class
		{
			var casted = expr as T;
			return casted != null ? func (casted) : null;
		}
	}
}

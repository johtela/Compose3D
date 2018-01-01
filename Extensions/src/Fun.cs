namespace Extensions
{
	using System;

	/// <summary>
	/// Extension methods for functions.
	/// </summary>
	public static class Fun
	{
		public static T Identity<T> (T arg)
		{
			return arg;
		}

		public static T Memoize<T> (Func<T> func, ref T store) where T : class
		{
			if (store == null)
				store = func ();
			return store;
		}

		public static T ToExpression<T> (Action action, T result)
		{
			action ();
			return result;
		}
	}
}

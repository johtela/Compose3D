namespace Extensions
{
	using System;

	public static class TupleExt
	{
		public static T First<T, U> (this Tuple<T, U> tuple)
		{
			return tuple.Item1;
		}

		public static U Second<T, U> (this Tuple<T, U> tuple)
		{
			return tuple.Item2;
		}

		public static Tuple<U, T> Swap<T, U> (this Tuple<T, U> tuple)
		{
			return Tuple.Create (tuple.Item2, tuple.Item1);
		}
	}
}

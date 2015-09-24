namespace Compose3D.Reactive
{
	using System;

	public delegate void Reaction<T> (T input);

	public static class Reaction
	{
		public static Reaction<T> Lift<T> (Action<T> action)
		{
			return input => action (input);
		}

		public static Reaction<T> Map<T, U> (this Reaction<U> reaction, Func<T, U> func)
		{
			return input => reaction (func (input));
		}

		public static Reaction<T> Filter<T> (this Reaction<T> reaction, Func<T, bool> predicate)
		{
			return input =>
			{
				if (predicate (input))
					reaction (input);
			};
		}

//		public static Reaction<T> MergeWith<T, U> (this Reaction<T> reaction, Reaction<U> other, Func<T, U, T> func)
//		{
////			return input => 
//		}
	}
}


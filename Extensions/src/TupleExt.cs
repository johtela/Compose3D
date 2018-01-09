/*
# Tuple Extension

The last set of extensions concerns tuples. These are mainly convenience 
methods making the code manipulating tuples easier and clearer.
*/
namespace Extensions
{
	using System;
	using System.Runtime.CompilerServices;

	public static class TupleExt
	{
		/*
		## Return the First and Second Item

		The members of the tuple are named pretty confusingly as ItemN where
		N is the index of the item. When these names are used in code they get
		easily mixed up, and the code becomes hard to read. An easy solution
		to this problem is to add replacement functions that have better names.
		*/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T First<T, U> (this Tuple<T, U> tuple)
		{
			return tuple.Item1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static U Second<T, U> (this Tuple<T, U> tuple)
		{
			return tuple.Item2;
		}
		/*
		## Swapping the Items

		The code for swapping items is not particularly long, but its intent
		might not be clear. So, we define an extension method which name tells
		exactly what operation is performed.
		*/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Tuple<U, T> Swap<T, U> (this Tuple<T, U> tuple)
		{
			return Tuple.Create (tuple.Item2, tuple.Item1);
		}
	}
}
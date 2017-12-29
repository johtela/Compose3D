/*
# Array extensions

This class contains extension methods for arrays. Most of the them are paremeterized by the
array item type, so they effectively work with any array.
*/
namespace Extensions
{
	using System;
	using System.Collections.Generic;
    using System.Runtime.CompilerServices;
	using System.Text;

	public static class ArrayExt
	{
        /* 
		## Retrieving Part of an Array
         
		The `Segment` method returns a part of an array as a new array. It allocates 
		a new array and uses the `Array.Copy` method to populate it. It also checks that 
		the starting index and the segment length are within the array bounds.
		*/
		public static T[] Segment<T> (this T[] array, int first, int length)
        {
            CheckBounds (array, first, length);
            var result = new T[length];
            Array.Copy (array, first, result, 0, length);
            return result;
        }
		/* 
		Sometimes the array part can just be returned as an `IEnumerable`. In that case 
		the `Slice` method can be used. It is more memory efficient than `Segment` since
		the items are returned lazily without copying them to a new array. 
		*/
        public static IEnumerable<T> Slice<T> (this T[] array, int first, int length)
        {
            CheckBounds (array, first, length);
            for (int i = 0; i < length; i++)
                yield return array[first + i];
        }

        private static void CheckBounds<T> (T[] array, int first, int length)
        {
            if (first < 0 || first >= array.Length)
                throw new ArgumentException ("First is out of array index range", "first");
            if (length < 0 || (first + length) > array.Length)
                throw new ArgumentException ("Length is out of array index range", "length");
        }
		/*
		## Reducing Arrays
        
		The reduce operation iterates through an array either from left to right or from
		right to left accumulating a value while doing so. The IEnumerable interface already 
		has the left reduce operation in the .NET framework, where the method is called 
		`Aggregate`. It always goes from left to right since there is no general way to 
		reverse an IEnumerable sequence. Arrays, however, can be efficiently traversed in 
		both directions, so it makes sense to implement these operations for them 
		specifically.
        
		The parameters of the reduce methods take an initial accumulator value, and a
		function which is called for each item in the array. The function gets the current
		item and the accumulator value, and returns a new value for the accumulator. After
		all items all traversed, the final accumulator value is returned.
        
		The reduce function (also known as `fold` in many functional languages) is 
		surprisingly powerful. You can actually implement almost all of the operations 
		that are commonly used to manipulate sequences using reduce alone. For example,
		mapping, filtering, reversing a sequence, and finding an item. It is basically the 
		functional version of the foreach loop.
		*/
        public static U ReduceLeft<T, U> (this T[] array, U acc, Func<U, T, U> func)
		{
			for (int i = 0; i < array.Length; i++)
				acc = func (acc, array[i]);
			return acc;
		}

		public static U ReduceRight<T, U> (this T[] array, Func<T, U, U> func, U acc)
		{
			for (int i = array.Length - 1; i >= 0; i--)
				acc = func (array[i], acc);
			return acc;
		}
		/*
		## Mapping Arrays

		Although mapping can be implemented using the reduce operation, it makes sense
		to define the operation separately. Again, the same operation is already found
		in the System.Linq namespace with the name `Select`, but arrays benefit from 
		knowing the result length in advance. The operation is more efficient when we
		allocate the result array right in the beginning.

		Map takes an array and a function that is called for each item in the array.
		The function returns the value copied to the result array.
		*/
		public static U[] Map<T, U> (this T[] vector, Func<T, U> func)
		{
			var result = new U[vector.Length];
			for (int i = 0; i < vector.Length; i++)
				result[i] = func (vector[i]);
			return result;
		}
		/*
		We can as easily define the map operation for two arrays as for one. In this
		case we take a function that returns the corresponding items in both arrays
		and returns the value copied to the result array. The same operation can be
		found in System.Linq namespace with the name `Zip`, but our implementation
		is more efficient for arrays.
		*/
		public static V[] Map2<T, U, V> (this T[] vector, U[] other, Func<T, U, V> func)
		{
			var result = new V[vector.Length];
			for (int i = 0; i < vector.Length; i++)
				result[i] = func (vector[i], other[i]);
			return result;
		}
		/*
		## Converting an Array to String

		This function is super useful in many situations, especially when outputting degugging
		data. The function returns the contents of an array as string using the `Object.ToString` 
		method to get the string representation of the indvidual items. The opening and closing 
		bracket, as well as the separator placed between the items, are given as parameters. 
		*/
		public static string ToString<T> (this T[] array, string openBracket, string closeBracket, 
            string separator)
		{
			StringBuilder sb = new StringBuilder (openBracket);

			for (int i = 0; i < array.Length; i++)
			{
				sb.Append (array[i]);

				if (i < (array.Length - 1))
					sb.Append (separator);
			}
			sb.Append (closeBracket);
			return sb.ToString ();
		}
		/*
		## Swapping Array Items

		A trivial but still useful operation is `Swap` which exchanges the contents of two
		array positions. This method does not check the index bounds in order not to waste
		cycles on something that the array indexing operations do anyway.
		*/
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Swap<T> (this T[] array, int i, int j)
		{
			var t = array[i];
			array[i] = array[j];
			array[j] = t;
		}
		/* 
		## Permutations

		A permutation is an unique ordering of a set of items. Sometimes you might need
		to get all the possible permutations of an array. For this purpose, we define 
		the `Permutations` operation. It uses 
		[Heap's algorithm](https://en.wikipedia.org/wiki/Heap%27s_algorithm) to recursively 
		generate all possible orderings. The algorithm is fairly simple, although due to 
		its recursive structure, it can be hard to follow. It is quite efficient, however, 
		mainly because it uses just a single copy of the source array to construct the 
		permutations.
		*/
        private static IEnumerable<T[]> EnumeratePermutations<T> (int n, T[] array)
		{
			if (n <= 0)
				yield return array;
			else
			{
				for (int i = 0; i <= n; i++)
				{
					foreach (var a in EnumeratePermutations (n - 1, array))
						yield return a;
					var j = n % 2 == 1 ? 0 : i;
					array.Swap (j, n);
				}
			}
		}

		public static IEnumerable<T[]> Permutations<T> (this T[] array)
		{
			var res = new T[array.Length];
			array.CopyTo (res, 0);
			return EnumeratePermutations (array.Length - 1, res);
		}
		/*
		## Combinations

		Contrary to permutation which takes an array of items and returns all its possible orderings,
		combination takes an array of IEnumerables and returns all possible ways the items in these
		IEnumerables can be combined together. The result is an IEnumerable of arrays with the same 
		length as the input array.

		The alogithm that generates the combinations is very similar to the permutation algorithm.
		It recursively selects each possible combination and uses only one result vector.
		*/
		private static IEnumerable<T[]> EnumerateCombinations<T>(this IEnumerable<T>[] values, T[] result,
			int index)
		{
			foreach (var x in values[index])
			{
				result[index] = x;
				if (index == values.Length - 1)
					yield return result;
				else
					foreach (var v in values.EnumerateCombinations(result, index + 1))
						yield return v;
			}
		}

		public static IEnumerable<T[]> Combinations<T>(this IEnumerable<T>[] vector)
		{
			return EnumerateCombinations(vector, new T[vector.Length], 0);
		}
		/*
		## Duplicating an Item

		A fairly common need is to create an array with the single item repeated _n_
		times. The `Duplicate` method does just that.
		*/
		public static T[] Duplicate<T> (this T value, int times)
		{
			var result = new T[times];
			for (int i = 0; i < times; i++)
				result[i] = value;
			return result;
		}
		/*
		## Simple Accessors

		The following three methods just make accessing an array a bit simpler and terser. 
		They are basically shorthands for common access patterns. The methods are decorated
		with a special attribute that hints to the compiler that they should be inlined for
		maximum performance.
		*/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T First<T>(this T[] array) => array[0];

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Last<T>(this T[] array) => array[array.Length - 1];

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int IndexOf<T>(this T[] array, T value) => Array.IndexOf(array, value);
		/*
		## Copying 2D Arrays

		There is no convenient and short way to copy an array with two dimensions.
		Therefore we provide the following `Copy` method.
		*/
		public static T[][] Copy<T> (this T[][] matrix)
		{
			var res = new T[matrix.Length][];
			for (int i = 0; i < matrix.Length; i++)
			{
				var len = matrix[i].Length;
				res[i] = new T[len];
				Array.Copy (matrix[i], res[i], len);
			}
			return res;
		}
		/*
		## Immutable Insertion and Replace

		There are situations where mutating an array is not possible, but instead
		a new copy should be created when an item is inserted or replaced. The 
		following two methods are there for these occasions.
		*/
		public static T[] Insert<T> (this T[] array, int index, T item)
		{
			if (index < 0 || index >= array.Length)
				throw new ArgumentOutOfRangeException ("index");
			var result = new T[array.Length];
			for (int i = 0; i < array.Length; i++)
				result[i] = 
					i < index ? array[i] :
					i == index ? item :	
					array[i - 1];
			return result;
		}

		public static T[] Replace<T> (this T[] array, int index, T item)
		{
			if (index < 0 || index >= array.Length)
				throw new ArgumentOutOfRangeException ("index");
			var result = (T[])array.Clone ();
			result[index] = item;
			return result;
		}
	}
}
/*
# Extensions for IEnumerable

Although .NET framework contains already a lot of extension methods for the 
`IEnumerable` interface we can still come up with new ones.
*/
namespace Extensions
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.CompilerServices;
	using Extensions;

	public static class EnumerableExt
	{
		/*
		## Checking If an IEnumerable Is Empty

		The `Any` method can be used to check if an IEnumerable contains any items.
		More often you need to check the inverse of that, whether it is empty.
		We define the `None` method to do that.
		*/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool None<T>(this IEnumerable<T> enumerable) =>
			!enumerable.Any();
		/*
		## Generating Ad-Hoc IEnumerable

		The easiest and fastest way to generate an IEnumerable with given items
		is to put them to an array. However, the syntax of declaring a new array
		is unnecessarily complex. Therefore we provide a trivial wrapper that 
		exploits the `params` array feature in C#. It simplifies the array 
		generation to:

			EnumerableExt.Enumerate (<item1>, <item2>, ...)
		*/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<T> Enumerate<T>(params T[] items) => items;
		/*
		## IEnumerable from a Function

		Another way to create an infinite IEnumerable is to wrap it over a function.
		*/
		public static IEnumerable<T> Generate<T>(Func<T> generator)
		{
			while (true)
				yield return generator();
		}
		/*
		## Consuming an Item

		Sometimes it is necessary to mutate an IEnumerable by extracting an item 
		from it. Of course this can be one using the `IEnumerator` interface, but 
		normally one does not want to deal with that interface explicitly. Even 
		when working with IEnumerator directly, extracting an item requires 
		multiple lines of code. So, we define a way to extract the next item 
		succinctly from an IEnumerator and an IEnumerable.
		*/
		public static T Next<T>(this IEnumerator<T> enumerator)
		{
			if (!enumerator.MoveNext())
				throw new ArgumentException("Enumerator exhausted");
			return enumerator.Current;
		}

		public static T Next<T>(ref IEnumerable<T> enumerable)
		{
			var result = enumerable.First();
			enumerable = enumerable.Skip(1);
			return result;
		}
		/*
		## Generic Printing

		By default the ToString() methods of any IEnumerable implementations do not
		return anything useful. To remedy this problem, we define a wrapper class that 
		evaluates the IEnumerable sequence, and uses the `Object.ToString` method to 
		output each item. The wrapper class is defined at the end of the file.
		*/
		public static IEnumerable<T> AsPrintable<T>(this IEnumerable<T> enumerable) => 
			new PrintableEnumerable<T>(enumerable);
		/*
		Another common use case is to output the enumeration as a string separating the 
		items by a specified characters, usually by ', '. For that purpose, we define
		the following method.
		*/
		public static string SeparateWith<T>(this IEnumerable<T> lines, string separator)
		{
			return lines.Any() ?
				lines.Select(l => l.ToString()).Aggregate((s1, s2) => s1 + separator + s2) :
				string.Empty;
		}
		/*
		## Appending and Prepending an Item

		Since IEnumerables are basically lazy iterators, it is possible to generically
		add or remove items to them without modifying the underlying data structure or 
		computation. We define two operations: `Append` which will add an item at the
		end of the enumeration, and `Prepend` which adds an item at the beginning.

		Note that the runtime cost incurred by these operations is very small since they
		are using the `Concat` method, which is very efficient both in terms of memory
		and execution time.
		*/
		public static IEnumerable<T> Append<T>(this IEnumerable<T> enumerable, T item) => 
			enumerable.Concat(Enumerate(item));

		public static IEnumerable<T> Prepend<T>(this IEnumerable<T> enumerable, T item) => 
			Enumerate(item).Concat(enumerable);
		/*
		## Repeating an IEnumerable

		You can put an IEnumerable on repeat so that you can loop through it indefinitely 
		with the following method.
		*/
		public static IEnumerable<T> Repeat<T>(this IEnumerable<T> enumerable)
		{
			while (true)
			{
				var enumerator = enumerable.GetEnumerator();
				while (enumerator.MoveNext())
					yield return enumerator.Current;
			}
		}
		/*
		## Enumerating a Range of Numbers

		There is already a `Range` extension method in the `System.Linq.Enumberable`
		class, but it only enumerates integers, and the step size cannot be changed. 
		So, let's fix these shortcomings.
		*/
		public static IEnumerable<float> Range(float start, float end, float step)
		{
			for (float val = start; step > 0 ? val <= end : val >= end; val += step)
				yield return val;
		}

		public static IEnumerable<int> Range(int start, int end, int step)
		{
			for (int val = start; step > 0 ? val <= end : val >= end; val += step)
				yield return val;
		}
		/*
		## Index of an Item

		Alhtough IEnumerables do not have indexes per se, it can be handy to check how far
		from a beginning of the sequence a specified item resides. The `FirstIndex` method
		returns the index of the first item matching the predicate. If no items match, -1 
		is returned.
		*/
		public static int FirstIndex<T>(this IEnumerable<T> items, Func<T, bool> predicate)
		{
			var result = 0;
			foreach (var item in items)
				if (predicate(item))
					return result;
				else
					result++;
			return -1;
		}
		/*
		## Minimum and Maximum Items

		The `Min` and `Max` methods in the Linq library return the minimum and maximum value of a 
		specified field. But what if you want to know the actual items that contained the minimum
		and maximum values instead of the values themselves. For that scenario, we define two 
		methods `MinItems` and `MaxItems` which return these items. Note that whereas `Min` and 
		`Max` return a single number, `MinItems` and `MaxItems` return potentially multiple items
		for which the selector returned the min/max value.
		*/
		public static IEnumerable<T> MinItems<T> (this IEnumerable<T> items, Func<T, float> selector)
		{
			var res = new List<T> ();
			var min = float.MaxValue;
			foreach (var item in items)
			{
				var value = selector (item);
				if (value < min)
				{
					min = value;
					res.Clear ();
					res.Add (item);
				}
				else if (value == min)
					res.Add (item);
			}
			return res;
		}

		public static IEnumerable<T> MaxItems<T> (this IEnumerable<T> items, Func<T, float> selector)
		{
			var res = new List<T> ();
			var max = float.MinValue;
			foreach (var item in items)
			{
				var value = selector (item);
				if (value.ApproxEquals (max))
					res.Add (item);
				else if (value > max)
				{
					max = value;
					res.Clear ();
					res.Add (item);
				}
			}
			return res;
		}
		/*
		## Removing Duplicates

		If you want to remove duplicates from an IEnumerable, you can use the `Distinct` method
		in the Linq library. But if your duplicates are already in consequtive positions, the
		method does way too much work. `RemoveConsequtiveDuplicates` handles this case in _O(n)_ 
		time. 
		*/
		public static IEnumerable<T> RemoveConsequtiveDuplicates<T> (this IEnumerable<T> items)
		{
			var prev = default (T);
			var first = true;
			foreach (var item in items)
			{
				if (first || !item.Equals (prev))
				{
					yield return item;
					first = false;
				}
				prev = item;
			}
		}
		/*
		## Copying Items to a 2D Array

		The `ToArray` method is one of the most (ab)used methods in Linq. It does not help, though,
		if you need to copy the contents to a two dimensional array.
		*/
		public static T[,] To2DArray<T>(this IEnumerable<T> enumerable, int dimension1, int dimension2)
		{
			var res = new T[dimension1, dimension2];
			var e = enumerable.GetEnumerator();
			for (int i = 0; i < dimension1; i++)
				for (int j = 0; j < dimension2; j++)
				{
					res[i, j] = e.Current;
					e.MoveNext();
				}
			return res;
		}
		/*
		## Returning Keys and Values

		The following two methods are shorthands that can be used when you need to extract
		keys of values from a `KeyValuePair` IEnumerable.
		*/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<K> Keys<K, V>(this IEnumerable<KeyValuePair<K, V>> pairs) => 
			pairs.Select(kv => kv.Key);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<V> Values<K, V>(this IEnumerable<KeyValuePair<K, V>> pairs) => 
			pairs.Select(kv => kv.Value);
	}
	/*
	## Printable IEnumerable

	The following class is a simple wrapper that overrides only the `ToString`
	method.
	*/
	public class PrintableEnumerable<T> : IEnumerable<T>
	{
		IEnumerable<T> _enumerable;

		public PrintableEnumerable(IEnumerable<T> enumerable)
		{
			_enumerable = enumerable;
		}

		public IEnumerator<T> GetEnumerator()
		{
			return _enumerable.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _enumerable.GetEnumerator();
		}

		public override string ToString()
		{
			return string.Format ("[ {0} ]", this.SeparateWith (", "));
		}
	}
}

/*
# Functional List

The data structure that is present in practically all functional languages 
is singly linked list. All operations defined for the list are immutable
meaning that they don't modify the existing list, but always return a new
version of it. Structural sharing makes the implementation of these 
operations quite efficient.

Since this data structure is useful in many algorithms and .NET framework 
does not provide one out-of-the-box, we define a minimal implementation 
here. We could use the semi-official Nuget package System.Collections.Immutable, 
but it brings with it a long chain of dependencies. So, we will roll our own 
implementation to limit the dependencies to .NET framework only.
*/
namespace Extensions
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Text;

	public class Seq<T> : IEnumerable<T>
	{
        public T First { get; private set; }
		public Seq<T> Rest { get; private set; }

		internal Seq (T first, Seq<T> rest)
		{
			First = first;
			Rest = rest;
		}

		internal Seq (T first)
		{
			First = first;
		}

		public static Seq<T> operator |(T first, Seq<T> rest) => Seq.Cons(first, rest);

		public static Seq<T> FromEnumerable (IEnumerable<T> enumerable)
        {
            Seq<T> prev = null, result = null;
            foreach (var item in enumerable)
            {
                var curr = new Seq<T> (item);
                if (prev == null)
                    result = curr;
                else
                    prev.Rest = curr;
                prev = curr;
            }
            return result;
        }

        public IEnumerator<T> GetEnumerator ()
		{
			for (var node = this; node != null; node = node.Rest)
				yield return node.First;
		}

		IEnumerator IEnumerable.GetEnumerator () => GetEnumerator();

		public string ToString(string openBracket, string separator, string closeBracket)
		{
			var res = new StringBuilder(openBracket);
			for (var item = this; item != null; item = item.Rest)
			{
				res.Append(item.First);
				if (item.Rest != null)
					res.Append(separator);
			}
			res.Append(closeBracket);
			return res.ToString();
		}

		public override string ToString() =>
			ToString("[ ", ", ", " ]");
	}

	public static class Seq
	{
        public static Seq<T> Cons<T> (T first) => 
            new Seq<T> (first);

        public static Seq<T> Cons<T> (T first, Seq<T> rest) => 
            new Seq<T> (first, rest);

        public static Seq<T> ToSeq<T> (this IEnumerable<T> enumerable)
        {
            return Seq<T>.FromEnumerable (enumerable);
        }

        public static Seq<T> Remove<T> (this Seq<T> seq, T item)
		{
			if (seq == null)
				throw new ArgumentException ("Item not found in sequence.");
			if (seq.First.Equals (item))
				return seq.Rest;
			return Cons (seq.First, Remove (seq.Rest, item));
		}

		public static bool Contains<T> (this Seq<T> seq, T item)
		{
            var s = seq;
            while (!(s == null || s.First.Equals (item)))
                s = s.Rest;
            return s != null;
		}
	}
}
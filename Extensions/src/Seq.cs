namespace Extensions
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;

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

        public static Seq<T> operator | (T first, Seq<T> rest)
        {
            return Seq.Cons (first, rest);
        }

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

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		public override string ToString ()
		{
			return "[ " + this.Select (ival => ival.ToString ()).Aggregate ((s1, s2) => s1 + ", " + s2) + " ]";
		}
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
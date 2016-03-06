namespace Compose3D.DataStructures
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	public class Seq<T> : IEnumerable<T>
	{
		public readonly T First;
		public readonly Seq<T> Rest;

		internal Seq (T first, Seq<T> rest)
		{
			First = first;
			Rest = rest;
		}

		internal Seq (T first)
		{
			First = first;
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
		public static Seq<T> Cons<T> (T first, Seq<T> rest)
		{
			return new Seq<T> (first, rest);
		}

		public static Seq<T> Cons<T> (T first)
		{
			return new Seq<T> (first);
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
			if (seq == null)
				return false;
			else if (seq.First.Equals (item))
				return true;
			else
				return Contains (seq.Rest, item);
		}
	}
}
namespace ComposeTester
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Compose3D;
	using Compose3D.Maths;
	using Compose3D.DataStructures;
	using LinqCheck;

	public class IntervalTreeTests
	{
		public static Gen<Interval<float, T>> GenInterval<T> (float minRange, float maxRange, float maxLen)
		{
			return from start in Gen.Choose (minRange, maxRange).ToFloat ()
				   from len in Gen.Choose (1, maxLen).ToFloat ()
				   select new Interval<float, T> (start, start + len, default (T));
		}

		public static Arbitrary<IntervalTree<float, T>> ArbitraryIntervalTree<T> (float minRange, float maxRange, float maxLen)
		{
			return new Arbitrary<IntervalTree<float, T>> (
				from ivals in GenInterval<T> (minRange, maxRange, maxLen).EnumerableOf ()
				select IntervalTree<float, T>.FromEnumerable (ivals),
				it => from e in Arbitrary.Get<IEnumerable<Interval<float, T>>> ().Shrink (it)
					  select IntervalTree<float, T>.FromEnumerable (e));
		}

		public void CheckInvariantsAndCount<T> ()
		{
			var prop =
				from it in Prop.ForAll (ArbitraryIntervalTree<T> (0f, 100f, 100f))
				select new { it };

			prop.Label ("Check tree invariants").Check (p => p.it.CheckInvariants ());
			prop.Label ("Check count is correct").Check (p => p.it.Count == p.it.Count ());
		}

		[Test]
		public void TestInvariantsAndCount ()
		{
			CheckInvariantsAndCount<int> ();
		}
	}
}

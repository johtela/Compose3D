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
		static IntervalTreeTests ()
		{
			Arbitrary.Register (new Arbitrary<Interval<float, int>> (GenInterval (0f, 10f, 10f)));
		}
		
		public static Gen<Interval<float, int>> GenInterval (float minRange, float maxRange, float maxLen)
		{
			return from start in Gen.Choose (minRange, maxRange).ToFloat ()
				   from len in Gen.Choose (1, maxLen).ToFloat ()
				   select new Interval<float, int> (start, start + len, 0);
		}

		public static Arbitrary<IntervalTree<float, int>> ArbitraryIntervalTree (float minRange, float maxRange, 
			float maxLen)
		{
			return new Arbitrary<IntervalTree<float, int>> (
				from ivals in GenInterval (minRange, maxRange, maxLen).EnumerableOf ()
				select IntervalTree<float, int>.FromEnumerable (ivals),
				it => from e in Arbitrary.Get<IEnumerable<Interval<float, int>>> ().Shrink (it)
					  select IntervalTree<float, int>.FromEnumerable (e));
		}

		public void CheckInvariantsAndCount ()
		{
			var prop =
				from it in Prop.ForAll (ArbitraryIntervalTree (0f, 100f, 100f))
				select new { it };

			prop.Label ("Check tree invariants").Check (p => p.it.CheckInvariants ());
			prop.Label ("Check count is correct").Check (p => p.it.Count == p.it.Count ());
		}

		[Test]
		public void TestInvariantsAndCount ()
		{
			CheckInvariantsAndCount ();
		}
	}
}

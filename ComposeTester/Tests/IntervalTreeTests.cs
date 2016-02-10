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
				   from data in Gen.Choose (0, int.MaxValue)
				   select new Interval<float, int> (start, start + len, data);
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

		[Test]
		public void TestInvariantsAndCount ()
		{
			var prop =
				from it in Prop.ForAll (ArbitraryIntervalTree (0f, 100f, 100f))
				select new { it };

			prop.Label ("Check tree invariants").Check (p => p.it.CheckInvariants ());
			prop.Label ("Count is correct").Check (p => p.it.Count == p.it.Count ());
			prop.Label ("At least one overlap").Check (
				p => (p.it.Count > 0).Implies (!p.it.Overlap (0f, 100f).IsEmpty ()));
		}

		[Test]
		public void TestAllPresent ()
		{
			var prop =
				from it in Prop.ForAll (ArbitraryIntervalTree (0f, 100f, 100f))
				let midpoints = (from ival in it
				                 select (ival.Low + ival.High) / 2f).AsPrintable ()
				select new { it, midpoints };
			
			prop.Label ("Check all present").Check (p => 
				p.midpoints.All (low => !p.it.Overlap (low, low + 1f).IsEmpty ()));
		}

		[Test]
		public void TestGapsNotOverlap ()
		{
			var prop =
				from it in Prop.ForAll (ArbitraryIntervalTree (0f, 100f, 100f))
				from low in Prop.ForAll (Gen.Choose (0.0, 100.0).ToFloat ())
				select new { it,  low };
			
			prop.Label ("No tree overlap => None of the intervals overlap").Check (
				p => p.it.Overlap (p.low, p.low + 1).IsEmpty ().Implies (
					p.it.All (ival => !ival.Overlap (p.low, p.low + 1))));
		}
		
		[Test]
		public void TestAdding ()
		{
			var prop =
				from it in Prop.ForAll (ArbitraryIntervalTree (0f, 100f, 100f))
				let cnt = it.Count
				from low in Prop.ForAll (Gen.Choose (200.0).ToFloat ())
				from len in Prop.ForAll (Gen.Choose (1.0, 100.0).ToFloat ())
				let high = low + len
				let newCnt = it.Add (low, high, 0)
				select new { it,  cnt, low, high, newCnt };

			prop.Label ("Count is correct").Check (p => p.newCnt == p.cnt + 1 && p.newCnt == p.it.Count ());
			prop.Label ("New range added").Check (p => p.it.Overlap (200f, 300f).Count () == 1);
			prop.Label ("No overlap above or below").Check (
				p => p.it.Overlap (-100f, 0f).IsEmpty () && p.it.Overlap (400f, 500f).IsEmpty ());
		}

		[Test]
		public void TestRemoving ()
		{
			var prop =
				from it in Prop.ForAll (ArbitraryIntervalTree (0f, 100f, 100f))
				let cnt = it.Count
				where cnt > 0
				from index in Prop.ForAll (Gen.Choose (0, cnt))
				let rem = it.Skip (index).First ()
				let newCnt = it.Remove (rem)
				let midpoints = (from ival in it
								 select (ival.Low + ival.High) / 2f).AsPrintable ()
				select new { it, cnt, index, rem, newCnt, midpoints };

			prop.Label ("Count is correct").Check (p => 
				p.newCnt == p.cnt - 1 && p.newCnt == p.it.Count ());
			prop.Label ("No overlap above or below").Check (p => 
				p.it.Overlap (-100f, 0f).IsEmpty () && p.it.Overlap (200f, 300f).IsEmpty ());
			prop.Label ("Removed interval not found").Check (p => 				
				p.it.All (ival => ival != p.rem));
			prop.Label ("Not removed intervals found").Check (p =>
				p.midpoints.All (low => !p.it.Overlap (low, low + 1f).IsEmpty ()));
		}
	}
}
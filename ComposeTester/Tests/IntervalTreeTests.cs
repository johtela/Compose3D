namespace ComposeTester
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Compose3D;
	using Compose3D.Maths;
	using Compose3D.DataStructures;
	using Extensions;
	using LinqCheck;

	public class IntervalTreeTests
	{
		public static Gen<Interval<float, int>> GenInterval (float minRange, float maxRange, float maxLen)
		{
			return from start in Gen.ChooseDouble (minRange, maxRange).ToFloat ()
				   from len in Gen.ChooseDouble (1, maxLen).ToFloat ()
				   from data in Gen.ChooseInt (0, int.MaxValue)
				   select new Interval<float, int> (start, start + len, data);
		}

		public static Arbitrary<IntervalTree<float, int>> ArbitraryIntervalTree (float minRange, float maxRange, 
			float maxLen)
		{
			return new Arbitrary<IntervalTree<float, int>> (
				from ivals in GenInterval (minRange, maxRange, maxLen).EnumerableOf ()
				select IntervalTree<float, int>.FromEnumerable (ivals),
				it => from ivals in it.ShrinkEnumerable ()
					  select IntervalTree<float, int>.FromEnumerable (ivals));
		}

		[Test]
		public void TestInvariantsAndCount ()
		{
			var prop =
				from it in Prop.ForAll (ArbitraryIntervalTree (0f, 100f, 100f))
				select it;

			prop.Label ("Check tree invariants").Check (it => it.CheckInvariants ());
			prop.Label ("Count is correct").Check (it => it.Count == it.Count ());
			prop.Label ("At least one overlap").Check (
				it => (it.Count > 0).Implies (!it.Overlap (0f, 100f).IsEmpty ()));
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
				from low in Prop.ForAll (Gen.ChooseDouble (0.0, 100.0).ToFloat ())
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
				from low in Prop.ForAll (Gen.ChooseDouble (200.0).ToFloat ())
				from len in Prop.ForAll (Gen.ChooseDouble (1.0, 100.0).ToFloat ())
				let high = low + len
				let ival = it.Add (low, high, 0)
				select new { it,  cnt, low, high, ival };

			prop.Label ("Count is correct").Check (p => p.cnt + 1 == p.it.Count ());
			prop.Label ("New range added").Check (p => p.it.Overlap (200f, 300f).Single () == p.ival);
			prop.Label ("No overlap above or below").Check (
				p => p.it.Overlap (-100f, 0f).IsEmpty () && p.it.Overlap (400f, 500f).IsEmpty ());
			prop.Label ("Visualize").Check (p => 
			{
				TestProgram.VConsole.ShowVisual (p.it.ToVisual ());
				return true;
			});
		}

		[Test]
		public void TestAddingDuplicate ()
		{
			var prop =
				from it in Prop.ForAll (ArbitraryIntervalTree (0f, 100f, 100f))
				let cnt = it.Count
				where cnt > 0
				from index in Prop.ForAll (Gen.ChooseInt (0, cnt))
				let ival = it.Skip (index).First ()
				let same = it.Add (ival.Low, ival.High, 42)
				select new { it, cnt, ival, same };

			prop.Label ("Count is correct").Check (p => p.cnt == p.it.Count ());
			prop.Label ("Same item added").Check (p => p.same == p.ival);
		}

		[Test]
		public void TestRemoving ()
		{
			var prop =
				from it in Prop.ForAll (ArbitraryIntervalTree (0f, 100f, 100f))
				let cnt = it.Count
				where cnt > 0
				from index in Prop.ForAll (Gen.ChooseInt (0, cnt))
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
		
		[Test]
		public void TestRemoveAll ()
		{
			var prop =
				from it in Prop.ForAll (ArbitraryIntervalTree (0f, 100f, 100f))
				let cnt = it.Count
				where cnt > 0
				let ivals = it.ToArray ()
				let checks = ivals.Select (ival => 
					{
						var c = it.Remove (ival);
						return Tuple.Create (c, it.CheckInvariants ());
					}).ToArray ()
				select new { it, cnt, ivals, checks };

			prop.Label ("Counts are correct").Check (p => 
				p.it.Count == 0 && p.it.Count () == 0 && 
				p.checks.First ().Item1 == p.cnt - 1 && p.checks.Last ().Item1 == 0);
			prop.Label ("Check invariants").Check (p => p.checks.All (t => t.Item2));
		}
		
		[Test]
		public void TestBigTree ()
		{
			var it = new IntervalTree<float, int> ();
			for (int i = 0; i < 100; i++)
			{
				it.Add (i, i + 1, 0);
				Check.IsTrue (it.CheckInvariants ());
			}
			var ita = it.ToArray ();
			for (int j = 0; j < 100; j += 2)
			{
				it.Remove (ita [j]);
				Check.IsTrue (it.CheckInvariants ());
			}
			TestProgram.VConsole.ShowVisual (it.ToVisual ());
		}
	}
}
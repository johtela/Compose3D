namespace ComposeTester
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Compose3D;
	using Compose3D.Maths;
	using Compose3D.DataStructures;
	using Extensions;
	using LinqCheck;

	public class BoundingTreeTests
	{
		static BoundingTreeTests ()
		{
			VecTests.Use ();
			Arbitrary.Register (new Arbitrary<Aabb<Vec2>> (GenAabb<Vec2> ()));
			Arbitrary.Register (new Arbitrary<Aabb<Vec3>> (GenAabb<Vec3> ()));
			Arbitrary.Register (new Arbitrary<KeyValuePair<Aabb<Vec2>, int>> (GenPair<Vec2, int> ()));
			Arbitrary.Register (new Arbitrary<KeyValuePair<Aabb<Vec3>, float>> (GenPair<Vec3, float> ()));
		}

		public static Gen<Aabb<V>> GenAabb<V> ()
			where V : struct, IVec<V, float>
		{
			var arb = Arbitrary.Get<V> ();
			return from low in arb.Generate
				   let high = low.Multiply (2f)
				   select new Aabb<V> (low, high);
		}

		public static Gen<KeyValuePair<Aabb<V>, T>> GenPair<V, T> ()
			where V : struct, IVec<V, float>
		{
			return from box in GenAabb<V> ()
				   from value in Arbitrary.Gen<T> ()
				   select new KeyValuePair<Aabb<V>, T> (box, value);
		}

		public static Arbitrary<B> ArbitraryBoundingTree<B, V, T> ()
			where B : IBoundingTree<V, T>, new ()
			where V : struct, IVec<V, float>
		{
			return new Arbitrary<B> (
				from pairs in GenPair<V, T> ().EnumerableOf ()
				select BoundingTree.FromEnumerable<B, V, T> (pairs),
				bt => from pairs in bt.ShrinkEnumerable ()
					  select BoundingTree.FromEnumerable<B, V, T> (pairs));
		}

		public void CheckAllPresent<B, V, T> ()
			where B : IBoundingTree<V, T>, new()
			where V : struct, IVec<V, float>
		{
			var prop = from bt in Prop.ForAll (ArbitraryBoundingTree<B, V, T> ())
					   let pairs = bt.ToArray ()
					   select new { bt, pairs };

			prop.Label ("{0}: All items are present", typeof (B)).Check (
				p => p.pairs.All (pair => p.bt.Overlap (pair.Key).Contains (pair)));
		}

		public void CheckAdding<B, V, T> ()
			where B : IBoundingTree<V, T>, new ()
			where V : struct, IVec<V, float>
		{
			var prop = from bt in Prop.ForAll (ArbitraryBoundingTree<B, V, T> ())
					   let cnt = bt.Count
					   from box in Prop.Choose<Aabb<V>> ()
					   from value in Prop.Choose<T> ()
					   let _ = Fun.ToExpression (() => bt.Add (box, value), 0)
					   select new { bt, cnt, box, value };

			prop.Label ("{0}: Count is correct", typeof (B)).Check (p => p.bt.Count == p.cnt + 1 && p.bt.Count () == p.bt.Count);
			prop.Label ("{0}: Item was added", typeof (B)).Check (p => p.bt.Overlap (p.box).Any (
				kv => kv.Key.Equals (p.box) && kv.Value.Equals (p.value)));
		}

		public void CheckRemoving<B, V, T> ()
			where B : IBoundingTree<V, T>, new()
			where V : struct, IVec<V, float>
		{
			var prop = from bt in Prop.ForAll (ArbitraryBoundingTree<B, V, T> ())
					   let cnt = bt.Count
					   where cnt > 0
					   from index in Prop.ForAll (Gen.Choose (0, cnt))
					   let rem = bt.Skip (index).First ()
					   let _ = Fun.ToExpression (() => bt.Remove (rem.Key, rem.Value), 0)
					   select new { bt, cnt, rem };

			prop.Label ("{0}: Count is correct", typeof (B)).Check (p => p.bt.Count == p.cnt - 1 && p.bt.Count () == p.bt.Count);
			prop.Label ("{0}: Item was removed", typeof (B)).Check (p => p.bt.Overlap (p.rem.Key).All (
				kv => !(kv.Key.Equals (p.rem.Key) && kv.Value.Equals (p.rem.Value))));
		}

		public void CheckRemoveAll<B, V, T> ()
			where B : IBoundingTree<V, T>, new()
			where V : struct, IVec<V, float>
		{
			var prop = from bt in Prop.ForAll (ArbitraryBoundingTree<B, V, T> ())
					   let cnt = bt.Count
					   let pairs = bt.Reverse ().ToArray ()
					   let cnts = pairs.Select (pair =>
					   {
						   bt.Remove (pair.Key, pair.Value);
						   return bt.Count;
					   }).ToArray ()
					   select new { bt, cnt, cnts, pairs };

			prop.Label ("{0}: Count is correct", typeof (B)).Check (p => 
				p.bt.Count == 0 && p.bt.Count () == 0 && 
				(p.cnt == 0 || (p.cnts.First () == p.cnt - 1 && p.cnts.Last () == 0)));
		}

		[Test]
		public void TestAllPresent ()
		{
			CheckAllPresent<BoundingRectTree<int>, Vec2, int> ();
			CheckAllPresent<BoundingBoxTree<float>, Vec3, float> ();
		}

		[Test]
		public void TestAdding ()
		{
			CheckAdding<BoundingRectTree<int>, Vec2, int> ();
			CheckAdding<BoundingBoxTree<float>, Vec3, float> ();
		}

		[Test]
		public void TestRemoving ()
		{
			CheckRemoving<BoundingRectTree<int>, Vec2, int> ();
			CheckRemoving<BoundingBoxTree<float>, Vec3, float> ();
		}

		[Test]
		public void TestRemoveAll ()
		{
			CheckRemoveAll<BoundingRectTree<int>, Vec2, int> ();
			CheckRemoveAll<BoundingBoxTree<float>, Vec3, float> ();
		}
	}
}

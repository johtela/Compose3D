namespace ComposeTester
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Compose3D;
	using Compose3D.Maths;
	using Compose3D.DataStructures;
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

		public void CheckAddition<B, V, T> ()
			where B : IBoundingTree<V, T>, new ()
			where V : struct, IVec<V, float>
		{
			var prop = from bt in Prop.ForAll (ArbitraryBoundingTree<B, V, T> ())
					   let cnt = bt.Count
					   from box in Prop.Choose<Aabb<V>> ()
					   from value in Prop.Choose<T> ()
					   let _ = Fun.ToExpression (() => bt.Add (box, value), 0)
					   select new { bt, cnt, box, value };

			prop.Label ("{0}: Count is correct", typeof(B)).Check (p => p.bt.Count == p.cnt + 1 && p.bt.Count () == p.bt.Count);

		}

		[Test]
		public void TestAddition ()
		{
			CheckAddition<BoundingRectTree<int>, Vec2, int> ();
			CheckAddition<BoundingBoxTree<float>, Vec3, float> ();
		}
	}
}

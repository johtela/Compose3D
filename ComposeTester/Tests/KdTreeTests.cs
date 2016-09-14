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

	class KdTreeTests
	{
		static KdTreeTests ()
		{
			Arbitrary.Register (ArbitraryKeyValuePair<Vec2, int> ());
			Arbitrary.Register (ArbitraryKeyValuePair<Vec3, float> ());
			Arbitrary.Register (ArbitraryKeyValuePair<Vec4, double> ());
		}

		public static Arbitrary<KeyValuePair<K, V>> ArbitraryKeyValuePair<K, V> ()
		{
			var arbKey = Arbitrary.Get<K> ();
			var arbValue = Arbitrary.Get<V> ();
			return new Arbitrary<KeyValuePair<K, V>> (
				from key in arbKey.Generate
				from value in arbValue.Generate
				select new KeyValuePair<K, V> (key, value),
				pair =>
					from key in arbKey.Shrink (pair.Key)
					select new KeyValuePair<K, V> (key, pair.Value));
		}

		public static Arbitrary<KdTree<V, T>> ArbitraryKdTree<V, T> ()
			where V : struct, IVec<V, float>
		{
			var arb = ArbitraryKeyValuePair<V, T> ();
			return new Arbitrary<KdTree<V, T>> (
				from pairs in arb.Generate.EnumerableOf ()
				select new KdTree<V, T> (pairs),
				tree => 
					from pairs in tree.ShrinkEnumerable ()
					select new KdTree<V, T> (pairs));
		}

		public void CheckConstructionAndCount<V, T> ()
			where V : struct, IVec<V, float>
		{
			var prop =
				from tree in Prop.ForAll (ArbitraryKdTree<V, T> ())
				select tree;

			prop.Label ("Count is correct").Check (tree => tree.Count == tree.Count ());
		}

		public void CheckAllPresent<V, T> ()
			where V : struct, IVec<V, float>
		{
			var prop =
				from tree in Prop.ForAll (ArbitraryKdTree<V, T> ())
				let pairs = tree.ToList ()
				select new { tree, pairs };

			prop.Label ("Check all present").Check (p =>
				p.pairs.All (pair => p.tree[pair.Key].Equals (pair.Value)));
		}

		public void CheckAdding<V, T> ()
			where V : struct, IVec<V, float>
		{
			var prop =
				from tree in Prop.ForAll (ArbitraryKdTree<V, T> ())
				let cnt = tree.Count
				from key in Prop.ForAll (Arbitrary.Get<V> ().Generate)
				from val in Prop.ForAll (Arbitrary.Get<T> ().Generate)
				let added = tree.TryAdd (key, val)
				select new { tree, cnt, key, val, added };

			prop.Label ("Count is correct").Check (p => !p.added || p.cnt + 1 == p.tree.Count ());
			prop.Label ("New value added").Check (p => !p.added || p.tree[p.key].Equals (p.val));
		}

		public void CheckAddingDuplicate<V, T> ()
			where V : struct, IVec<V, float>
		{
			var prop =
				from tree in Prop.ForAll (ArbitraryKdTree<V, T> ())
				let cnt = tree.Count
				where cnt > 0
				from index in Prop.ForAll (Gen.Choose (0, cnt))
				let pair = tree.Skip (index).First ()
				let same = !tree.TryAdd (pair.Key, pair.Value)
				select new { tree, cnt, pair, same };

			prop.Label ("Count is correct").Check (p => p.cnt == p.tree.Count ());
			prop.Label ("Same item added").Check (p => p.same);
		}

		public void CheckRemoving<V, T> ()
			where V : struct, IVec<V, float>
		{
			T value;
			var prop =
				from tree in Prop.ForAll (ArbitraryKdTree<V, T> ())
				let cnt = tree.Count
				where cnt > 0
				from index in Prop.ForAll (Gen.Choose (0, cnt))
				let pair = tree.Skip (index).First ()
				let removed = tree.TryRemove (pair.Key, out value)
				select new { tree, cnt, index, pair, removed };

			prop.Label ("Count is correct").Check (p =>
				p.removed && p.tree.Count () == p.cnt - 1);
			prop.Label ("Removed psotion not found").Check (p =>
				!p.tree.Contains (p.pair.Key));
		}

		public void CheckOverlap<V, T> ()
			where V : struct, IVec<V, float>
		{
			var prop =
				from tree in Prop.ForAll (ArbitraryKdTree<V, T> ())
				let cnt = tree.Count
				where cnt > 1
				from index1 in Prop.ForAll (Gen.Choose (0, cnt))
				from index2 in Prop.ForAll (Gen.Choose (0, cnt))
				let pos1 = tree.Skip (index1).First ().Key
				let pos2 = tree.Skip (index2).First ().Key
				let bbox = new Aabb<V> (pos1, pos2)
				let overlapping = tree.Overlap (bbox).AsPrintable ()
				select new { tree, bbox, overlapping };

			prop.Label ("If bounding box is not a point, then at least two items overlap").Check (p => 
				(p.bbox.Min.Equals (p.bbox.Max) && p.overlapping.Count () == 1) ||
				p.overlapping.Count () >= 2);
		}

		[Test]
		public void TestConstructionAndCount ()
		{
			CheckConstructionAndCount<Vec2, int> ();
			CheckConstructionAndCount<Vec3, float> ();
			CheckConstructionAndCount<Vec4, double> ();
		}

		[Test]
		public void TestAllPresent ()
		{
			CheckAllPresent<Vec2, int> ();
			CheckAllPresent<Vec3, float> ();
			CheckAllPresent<Vec4, double> ();
		}

		[Test]
		public void TestAdding ()
		{
			CheckAdding<Vec2, int> ();
			CheckAdding<Vec3, float> ();
			CheckAdding<Vec4, double> ();
			CheckAddingDuplicate<Vec2, int> ();
			CheckAddingDuplicate<Vec3, float> ();
			CheckAddingDuplicate<Vec4, double> ();
		}

		[Test]
		public void TestRemoving ()
		{
			CheckRemoving<Vec2, int> ();
			CheckRemoving<Vec3, float> ();
			CheckRemoving<Vec4, double> ();
		}

		[Test]
		public void TestOverlap ()
		{
			CheckOverlap<Vec2, int> ();
			CheckOverlap<Vec3, float> ();
			CheckOverlap<Vec4, double> ();
		}
	}
}

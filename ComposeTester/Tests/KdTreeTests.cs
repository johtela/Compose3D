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
		public static Arbitrary<KdTree<V, T>> ArbitraryKdTree<V, T> ()
			where V : struct, IVec<V, float>
		{
			var arb = Arbitrary.Get <KeyValuePair<V, T>> ();
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
				from index in Prop.ForAll (Gen.ChooseInt (0, cnt))
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
				from index in Prop.ForAll (Gen.ChooseInt (0, cnt))
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
				from index1 in Prop.ForAll (Gen.ChooseInt (0, cnt))
				from index2 in Prop.ForAll (Gen.ChooseInt (0, cnt))
				let pos1 = tree.Skip (index1).First ().Key
				let pos2 = tree.Skip (index2).First ().Key
				let bbox = new Aabb<V> (pos1, pos2)
				let overlapping = tree.Overlap (bbox).AsPrintable ()
				select new { tree, bbox, overlapping };

			prop.Label ("If bounding box is not a point, then at least two items overlap").Check (p => 
				(p.bbox.Min.Equals (p.bbox.Max) && p.overlapping.Count () == 1) ||
				p.overlapping.Count () >= 2);
		}

		public void CheckNearestNeighbour<V, T> (Func<V, V, float> distance, string distDesc)
			where V : struct, IVec<V, float>
		{
			var prop =
				from tree in Prop.ForAll (ArbitraryKdTree<V, T> ())
				from pos in Prop.ForAll (Arbitrary.Get<V> ())
				from num in Prop.ForAll (Gen.ChooseInt (1, 4))
				let nearest = tree.NearestNeighbours (pos, num, distance).AsPrintable ()
				let lastBest = nearest.None () ?
					float.PositiveInfinity :
					distance (nearest.Last ().Key, pos)
				select new { tree, pos, num, nearest, lastBest };

			prop.Label ("Not neighbours have longer distance: {0}, {1}", typeof(V).Name, distDesc)
				.Check (p => p.tree.Count == 0 || p.tree.All (pair => 
				p.nearest.Contains (pair) || p.lastBest  <= distance (pair.Key, p.pos)));
			prop.Label ("Neighbours in right order: {0}, {1}", typeof (V).Name, distDesc)
				.Check (p => p.nearest.Zip (p.nearest.Skip (1),
					(p1, p2) => distance (p1.Key, p.pos) <= distance (p2.Key, p.pos))
					.All (Fun.Identity));
//			prop.Label ("Visualize").Check (p =>
//			{
//				TestProgram.VConsole.ShowVisual (p.tree.ToVisual ());
//				return true;
//			});
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

		[Test]
		public void TestNearestNeigbour ()
		{
			CheckNearestNeighbour<Vec2, int> (Vec.SquaredDistanceTo<Vec2>, "euclidean");
			CheckNearestNeighbour<Vec3, float> (Vec.SquaredDistanceTo<Vec3>, "euclidean");
			CheckNearestNeighbour<Vec4, double> (Vec.SquaredDistanceTo<Vec4>, "euclidean");
			CheckNearestNeighbour<Vec2, int> (Vec.ManhattanDistanceTo<Vec2>, "manhattan");
			CheckNearestNeighbour<Vec3, float> (Vec.ManhattanDistanceTo<Vec3>, "manhattan");
			CheckNearestNeighbour<Vec4, double> (Vec.ManhattanDistanceTo<Vec4>, "manhattan");
		}
	}
}

namespace Compose3D.DataStructures
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using Visuals;
	using Extensions;
	using Maths;

	public class KdTree<V, T> : IEnumerable<KeyValuePair<V, T>>
		where V : struct, IVec<V, float>
	{
		private class KdNode
		{
			public readonly V Position;
			public readonly T Data;
			public KdNode Left;
			public KdNode Right;

			public KdNode (V position, T data)
			{
				Position = position;
				Data = data;
			}
		}

		private int _count;
		private KdNode _root;

		public KdTree () { }

		public KdTree (IEnumerable<KeyValuePair<V, T>> values)
		{
			var t = ConstructTree (values, 0);
			_root = t.Item1;
			_count = t.Item2;
		}

		private static Tuple<KdNode, int> ConstructTree (IEnumerable<KeyValuePair<V, T>> values, int depth)
		{
			KdNode tree = null;
			var count = 0;
			foreach (var pair in values)
			{
				bool added = false;
				tree = AddNode (tree, new KdNode (pair.Key, pair.Value), depth, ref added);
				if (added)
					count++;
			}
			return Tuple.Create (tree, count);
		}

		private static IEnumerable<KdNode> EnumerateTree (KdNode node)
		{
			if (node == null)
				yield break;
			var path = new Stack<KdNode> ();
			path.Push (node);
			while (path.Count > 0)
			{
				var curr = path.Pop ();
				yield return curr;
				if (curr.Right != null)
					path.Push (curr.Right);
				if (curr.Left != null)
					path.Push (curr.Left);
			}
		}

		private static KdNode FindNode (KdNode tree, V pos, int depth)
		{
			if (tree == null)
				return null;
			if (tree.Position.Equals (pos))
				return tree;
			var k = depth % pos.Dimensions;
			return FindNode (
				pos[k] < tree.Position[k] ? tree.Left : tree.Right, 
				pos, depth + 1);
		}

		private static KdNode AddNode (KdNode tree, KdNode node, int depth, ref bool added)
		{
			if (tree == null)
			{
				added = true;
				return node;
			}
			if (!tree.Position.Equals (node.Position))
			{
				var k = depth % node.Position.Dimensions;
				if (node.Position[k] < tree.Position[k])
					tree.Left = AddNode (tree.Left, node, depth + 1, ref added);
				else
					tree.Right = AddNode (tree.Right, node, depth + 1, ref added);
			}
			return tree;
		}

		private static KdNode RemoveNode (KdNode tree, V pos, int depth, out KdNode removed)
		{
			if (tree == null)
			{
				removed = null;
				return null;
			}
			if (tree.Position.Equals (pos))
			{
				removed = tree;
				return ConstructTree (
					EnumerateTree (tree).Skip (1).Select (
						node => new KeyValuePair<V, T> (node.Position, node.Data)),
					depth).Item1;
			}
			var k = depth % pos.Dimensions;
			if (pos[k] < tree.Position[k])
				tree.Left = RemoveNode (tree.Left, pos, depth + 1, out removed);
			else
				tree.Right = RemoveNode (tree.Right, pos, depth + 1, out removed);
			return tree;
		}

		private static IEnumerable<KdNode> OverlappingNodes (KdNode tree, Aabb<V> bbox, int depth)
		{
			if (tree == null)
				yield break;
			if (bbox & tree.Position)
				yield return tree;
			var k = depth % tree.Position.Dimensions;
			if (bbox.Min[k] < tree.Position[k])
				foreach (var node in OverlappingNodes (tree.Left, bbox, depth + 1))
					yield return node;
			if (bbox.Max[k] >= tree.Position[k])
				foreach (var node in OverlappingNodes (tree.Right, bbox, depth + 1))
					yield return node;
		}

		private static KdNode NearestNeighbour (KdNode tree, V pos, Aabb<V> bounds, int depth, KdNode best,
			Func<V, V, float> distance)
		{
			if (tree != null)
			{
				var k = depth % tree.Position.Dimensions;
				var split = tree.Position[k];
				var leftBounds = new Aabb<V> (bounds.Min, bounds.Max.With (k, split));
				var rightBounds = new Aabb<V> (bounds.Min.With (k, split), bounds.Max);
				if (pos[k] < split)
				{
					best = NearestNeighbour (tree.Left, pos, leftBounds, depth + 1, best, distance);
					if (best == null || distance (tree.Position, pos) < distance (best.Position, tree.Position))
						best = tree;
					if (rightBounds.Corners.Any (c => distance (c, pos) < distance (best.Position, tree.Position)))
						best = NearestNeighbour (tree.Right, pos, rightBounds, depth + 1, best, distance);
				}
				else
				{
					best = NearestNeighbour (tree.Right, pos, rightBounds, depth + 1, best, distance);
					if (best == null || distance (tree.Position, pos) < distance (best.Position, tree.Position))
						best = tree;
					if (rightBounds.Corners.Any (c => distance (c, pos) < distance (best.Position, tree.Position)))
						best = NearestNeighbour (tree.Right, pos, rightBounds, depth + 1, best, distance);
				}
			}
			return best;
		}

		public bool TryAdd (V pos, T data)
		{
			bool added = false;
			_root = AddNode (_root, new KdNode (pos, data), 0, ref added);
			if (added)
				_count++;
			return added;
		}

		public void Add (V pos, T data)
		{
			if (!TryAdd (pos, data))
				throw new InvalidOperationException ("Attempt to add duplicate position to the tree.");
		}

		public bool TryRemove (V pos, out T value)
		{
			KdNode removed;
			_root = RemoveNode (_root, pos, 0, out removed);
			if (removed != null)
			{
				_count--;
				value = removed.Data;
				return true;
			}
			else
			{
				value = default (T);
				return false;
			}
		}

		public T Remove (V pos)
		{
			T value;
			if (TryRemove (pos, out value))
				return value;
			else
				throw new KeyNotFoundException ("Given position is not in the tree.");
		}

		public bool TryFind (V position, out T value)
		{
			var node = FindNode (_root, position, 0);
			if (node != null)
			{
				value = node.Data;
				return true;
			}
			else
			{
				value = default (T);
				return false;
			}
		}

		public bool Contains (V position)
		{
			return FindNode (_root, position, 0) != null;
		}

		public IEnumerable<KeyValuePair<V, T>> Overlap (Aabb<V> bbox)
		{
			return OverlappingNodes (_root, bbox, 0)
				.Select (node => new KeyValuePair<V, T> (node.Position, node.Data));
		}

		KeyValuePair<V, T> NearestNeighbour (V pos, Func<V, V, float> distance)
		{
			var bounds = new Aabb<V> (
				Vec.New<V, float> (float.NegativeInfinity),
				Vec.New<V, float> (float.PositiveInfinity));
			var best = NearestNeighbour (_root, pos, bounds, 0, null, distance);
			return new KeyValuePair<V, T> (best.Position, best.Data);
		}

		public int Count
		{
			get { return _count; }
		}

		public T this[V position]
		{
			get
			{
				var node = FindNode (_root, position, 0);
				if (node != null)
					return node.Data;
				else
					throw new KeyNotFoundException ("Given position is not in the tree.");
			}
		}

		public override string ToString ()
		{
			return _root == null ? "[ ]" :
				"[ " + this.Select (pair => pair.ToString ()).Aggregate ((s1, s2) => s1 + ", " + s2) + " ]";
		}

		#region IEnumerable implementation

		public IEnumerator<KeyValuePair<V, T>> GetEnumerator ()
		{
			return EnumerateTree (_root).Select (
				node => new KeyValuePair<V, T> (node.Position, node.Data))
				.GetEnumerator ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		#endregion
	}
}

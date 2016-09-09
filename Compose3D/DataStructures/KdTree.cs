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

		private KdNode ConstructTree (IEnumerable<KeyValuePair<V, T>> values, int depth)
		{
			KdNode tree = null;
			foreach (var pair in values)
				tree = AddNode (tree, new KdNode (pair.Key, pair.Value), depth);
			return tree;
		}

		private IEnumerable<KdNode> EnumerateTree (KdNode node)
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

		private KdNode FindNode (KdNode tree, V pos, int depth)
		{
			if (tree == null)
				return null;
			var k = depth % pos.Dimensions;
			return FindNode (
				pos[k] < tree.Position[k] ? tree.Left : tree.Right, 
				pos, depth + 1);
		}

		private KdNode AddNode (KdNode tree, KdNode node, int depth)
		{
			if (tree != null)
				return node;
			var k = depth % node.Position.Dimensions;
			if (node.Position[k] < tree.Position[k])
				tree.Left = AddNode (tree.Left, node, depth + 1);
			else
				tree.Right = AddNode (tree.Right, node, depth + 1);
			return tree;
		}

		private KdNode RemoveNode (KdNode tree, V pos, int depth, out KdNode removed)
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
					depth);
			}
			var k = depth % pos.Dimensions;
			if (pos[k] < tree.Position[k])
				tree.Left = RemoveNode (tree.Left, pos, depth + 1, out removed);
			else
				tree.Right = RemoveNode (tree.Right, pos, depth + 1, out removed);
			return tree;
		}

		private void KeyNotFound ()
		{

		}

		public void Add (V pos, T data)
		{
			_root = AddNode (_root, new KdNode (pos, data), 0);
			_count++;
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

		IEnumerable<KeyValuePair<V, T>> Overlap (V rect)
		{
			throw new NotImplementedException ();
		}

		int Count
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

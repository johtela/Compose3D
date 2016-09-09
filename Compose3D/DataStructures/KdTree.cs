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

			public Tuple<KdNode, int> FindNode (V pos, int depth)
			{
				var k = depth % pos.Dimensions;
				var child = Position[k] < pos[k] ? Left : Right;
				return child != null ? 
					child.FindNode (pos, depth + 1) : 
					Tuple.Create (this, k);
			}
		}

		private int _count;
		private KdNode _root;

		int Count
		{
			get { return _count; }
		}

		public void Add (V pos, T data)
		{
			var newNode = new KdNode (pos, data);
			if (_root == null)
				_root = newNode;
			_root.FindNode (pos, 0).Bind ((parent, k) =>
			{
				if (parent.Position[k] < pos[k])
					parent.Left = newNode;
				else
					parent.Right = newNode;
			});
		}

		public bool TryRemove (V pos, out T value)
		{
			value = default (T);
			if (_root == null)
				return false;
			var t = _root.FindNode (pos, 0);
			var node = t.Item1;
			if (node != null && node.Position.Equals (pos))
			{
				//TODO: remove node from parent.
				value = node.Data;
				return true;
			}
			else
				return false;
		}

		IEnumerable<KeyValuePair<V, T>> Overlap (V rect)
		{
			throw new NotImplementedException ();
		}

		public IEnumerator<KeyValuePair<V, T>> GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			throw new NotImplementedException ();
		}
	}
}

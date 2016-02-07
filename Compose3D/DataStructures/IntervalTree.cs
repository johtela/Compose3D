namespace Compose3D.DataStructures
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;

	public class Interval<N, T> where N : struct, IComparable<N>
	{
		internal Interval<N, T> _left;
		internal Interval<N, T> _right;
		internal N _max;
		
		public Interval (N low, N high, T data)
		{
			Low = low;
			High = high;
			Data = data;
			_max = high;
		}

		internal void UpdateMax ()
		{
			_max = High;
			if (_left != null && _max.CompareTo (_left._max) < 0)
				_max = _left._max;
			if (_right != null && _max.CompareTo (_right._max) < 0)
				_max = _right._max;
		}
		
		internal void AssignFrom (Interval<N, T> other)
		{
			Low = other.Low;
			High = other.High;
			Data = other.Data;
		}
		
		internal void WalkInPostOrder (Action<Interval<N, T>> action)
		{
			if (_left != null)
				_left.WalkInPostOrder (action);
			if (_right != null)
				_right.WalkInPostOrder (action);
			action (this);
		}

		internal void CheckInvariants ()
		{
			if (_left != null)
			{
				Debug.Assert (_left.Low.CompareTo (Low) <= 0, "Low order wrong in left child");
				Debug.Assert (_left._max.CompareTo (_max) <= 0, "Max bigger in left child than in parent");
			}
			if (_right != null)
			{
				Debug.Assert (_right.Low.CompareTo (Low) >= 0, "Low order wrong in right child");
				Debug.Assert (_right._max.CompareTo (_max) <= 0, "Max bigger in right child than in parent");
			}
		}

		public bool Intersect (N low, N high)
		{
			return High.CompareTo (low) >= 0 && Low.CompareTo (high) <= 0;
		}

		public N Low { get; private set; }
		public N High { get; private set; }
		public T Data { get; private set; }
	}
	
	public class IntervalTree<N, T> where N : struct, IComparable<N>
	{	
		private Interval<N, T> _root;
		
		public void Add (N low, N high, T data)
		{
			var newNode = new Interval<N, T> (low, high, data);
			if (_root == null)
			{
				_root = newNode;
				return;
			}			
			var node = _root;
			// Find the parent node and update the max interval end on the way.
			while (true)
			{
		 		if (high.CompareTo (node._max) > 0)
					node._max = high;
				if (low.CompareTo (node.Low) < 0)
				{
					if (node._left == null)
					{
						node._left = newNode;
						return;
					}
					node = node._left;
				}
				else
				{
					if (node._right == null)
					{
						node._right = newNode;
						return;
					}
					node = node._right;
				}
			}
		}

		private Stack<Interval<N, T>> PathToNode (Interval<N, T> node)
		{
			var result = new Stack<Interval<N, T>> ();
			// Push null as the parent of the root.
			result.Push (null);
			if (_root != node)
			{
				var current = _root;
				do
				{
					result.Push (current);
					current = node.Low.CompareTo (current.Low) < 0 ?
						current._left : current._right;
				}
				while (current != node);
			}
			return result;
		}
		
		private void ChangeChild (Interval<N, T> parent, Interval<N, T> node, Interval<N, T> newNode)
		{
			if (parent == null)
				_root = newNode;
			else if (node == parent._left)
				parent._left = newNode;
			else
				parent._right = newNode;
		}
		
		public void Remove (Interval<N, T> node)
		{
			var path = PathToNode (node);
			if (node._left == null || node._right == null)
				ChangeChild (path.Peek (), node, node._left ?? node._right);
			else
			{
				path.Push (node);
				var succ = node._right;
				while (succ._left != null)
				{
					path.Push (succ);
					succ = succ._left;
				}
				var succParent = path.Peek ();
				if (succParent != node)
					succParent._left = succ._right;
				else
					node._right = succ._right;
				node.AssignFrom (succ);
			}
			while (path.Peek () != null)
				path.Pop ().UpdateMax ();
		}
		
		public IEnumerable<Interval<N, T>> Intersect (N low, N high)
		{
			var stack = new Stack<Interval<N, T>> ();
			stack.Push (_root);
			while (stack.Count > 0)
			{
				var node = stack.Pop ();
				if (node.Intersect (low, high))
					yield return node;
				if (node._left != null && node._left._max.CompareTo (low) >= 0)
					stack.Push (node._left);
				if (node._right != null && high.CompareTo (node.Low) >= 0)
					stack.Push (node._right);
			}
		}
		
		public void CheckInvariants ()
		{
			_root.WalkInPostOrder (node => node.CheckInvariants ());
		}
	}
}
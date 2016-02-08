namespace Compose3D.DataStructures
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;

	public class Interval<N, T> where N : struct, IComparable<N>
	{
		internal Interval<N, T> _left;
		internal Interval<N, T> _right;
		internal N _max;
		
		public Interval (N low, N high, T data)
		{
			if (low.CompareTo (high) > 0)
				throw new ArgumentException ("low parameter must be less or equal than high", "low");
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
		
		internal bool CheckInvariants ()
		{
			if (_left != null &&
				(_left.Low.CompareTo (Low) > 0 || _left._max.CompareTo (_max) > 0))
				return false;
			if (_right != null &&
				(_right.Low.CompareTo (Low) < 0 || _right._max.CompareTo (_max) > 0))
				return false;
			return true;
		}
		
		public bool Intersect (N low, N high)
		{
			return High.CompareTo (low) >= 0 && Low.CompareTo (high) <= 0;
		}

		public override string ToString ()
		{
			return string.Format ("({0}, {1}): {2}", Low, High, _max);
		}

		public N Low { get; private set; }
		public N High { get; private set; }
		public T Data { get; private set; }
	}
	
	public class IntervalTree<N, T> : IEnumerable<Interval<N, T>>
		where N : struct, IComparable<N>
	{	
		private Interval<N, T> _root;
		private int _count;
		
		public void Add (N low, N high, T data)
		{
			Add (new Interval<N, T> (low, high, data));
		}

		public void Add (Interval<N, T> interval)
		{
			_count++;
			_root = AddNode (_root, interval);
		}
		
		private Interval<N, T> AddNode (Interval<N, T> node, Interval<N, T> newNode)
		{
			if (node == null)
				return newNode;
			if (newNode.Low.CompareTo (node.Low) < 0)
				node._left = AddNode (node._left, newNode);
			else
				node._right = AddNode (node._right, newNode);
			if (newNode.High.CompareTo (node._max) > 0)
				node._max = newNode.High;
			return node;
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
			_count--;
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
		
		public bool CheckInvariants ()
		{
			return this.All (ival => ival.CheckInvariants ());
		}

		public static IntervalTree<N, T> FromEnumerable (IEnumerable<Interval<N, T>> values)
		{
			var result = new IntervalTree<N, T> ();
			foreach (var item in values)
				result.Add (item.Low, item.High, item.Data);
			return result;
		}

		public int Count
		{
			get { return _count; }
		}

		public IEnumerator<Interval<N, T>> GetEnumerator ()
		{
			var stack = new Stack<Interval<N, T>> ();

			for (var current = _root; current != null || stack.Count > 0; current = current._right)
			{
				while (current != null)
				{
					stack.Push (current);
					current = current._left;
				}
				current = stack.Pop ();
				yield return current;
			}
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}
		
		public override string ToString ()
		{
			return "[ " + this.Select (ival => ival.ToString ()).Aggregate ((s1, s2) => s1 + ", " + s2) + " ]";
		}
	}
}
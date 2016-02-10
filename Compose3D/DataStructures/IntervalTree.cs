namespace Compose3D.DataStructures
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;

	public class Interval<N, T> where N : struct, IComparable<N>
	{
		public readonly N Low;
		public readonly N High;
		public readonly T Data;

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
		
		internal Interval<N, T> AssignFrom (Interval<N, T> other)
		{
			var result = new Interval<N, T> (other.Low, other.High, other.Data);
			result._left = _left;
			result._right = _right;
			return result;
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
		
		public bool Overlap (N low, N high)
		{
			return High.CompareTo (low) >= 0 && Low.CompareTo (high) <= 0;
		}

		public override string ToString ()
		{
			return string.Format ("({0}, {1}): {2}", Low, High, _max);
		}
	}
	
	public class IntervalTree<N, T> : IEnumerable<Interval<N, T>>
		where N : struct, IComparable<N>
	{	
		private Interval<N, T> _root;
		private int _count;
		private int _version;
		
		public int Add (N low, N high, T data)
		{
			return Add (new Interval<N, T> (low, high, data));
		}

		public int Add (Interval<N, T> interval)
		{
			_root = AddNode (_root, interval);
			_version++;
			return ++_count;
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

		public int Remove (Interval<N, T> node)
		{
			_root = RemoveNode (_root, node);
			_version++;
			return --_count;
		}

		public Interval<N, T> RemoveNode (Interval<N, T> current, Interval<N, T> node)
		{
			if (current == null)
				return null;
			if (current == node)
			{
				if (current._left == null || current._right == null)
					return current._left ?? current._right;
				else
				{
					var succ = current._right;
					while (succ._left != null)
						succ = succ._left;
					current = current.AssignFrom (succ);
					current._right = RemoveNode (current._right, succ);
				}
			}
			else if (node.Low.CompareTo (current.Low) < 0)
				current._left = RemoveNode (current._left, node);
			else
				current._right = RemoveNode (current._right, node);
			current.UpdateMax ();
			return current;
		}
		
		public IEnumerable<Interval<N, T>> Overlap (N low, N high)
		{
			if (_root == null)
				yield break;
			var stack = new Stack<Interval<N, T>> ();
			stack.Push (_root);
			while (stack.Count > 0)
			{
				var node = stack.Pop ();
				if (node.Overlap (low, high))
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
			var version = _version;

			for (var current = _root; current != null || stack.Count > 0; current = current._right)
			{
				while (current != null)
				{
					stack.Push (current);
					current = current._left;
				}
				current = stack.Pop ();
				if (_version != version)
					throw new InvalidOperationException ("Underlying tree has changed while enumerating it.");
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
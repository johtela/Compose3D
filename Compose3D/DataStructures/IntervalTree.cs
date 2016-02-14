namespace Compose3D.DataStructures
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using Visuals;

	public class Interval<N, T> where N : struct, IComparable<N>
	{
		public readonly N Low;
		public readonly N High;
		public T Data;

		internal Interval<N, T> _left;
		internal Interval<N, T> _right;
		internal N _max;
		internal bool _isRed;
		
		public Interval (N low, N high, T data)
		{
			if (low.CompareTo (high) > 0)
				throw new ArgumentException ("low parameter must be less or equal than high", "low");
			Low = low;
			High = high;
			Data = data;
			_max = high;
			_isRed = true;
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
			return !(_isRed && ((_left != null && _left._isRed) || (_right != null && _right._isRed)));
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
	
	public class IntervalTree<N, T> : IEnumerable<Interval<N, T>>, IVisualizable
		where N : struct, IComparable<N>
	{	
		private Interval<N, T> _root;
		private int _count;
		private int _version;

		private bool IsRed (Interval<N, T> node)
		{
			return node != null ? node._isRed : false;
		}

		private Interval<N, T> RotateLeft (Interval<N, T> node)
		{
			var x = node._right;
			node._right = x._left;
			x._left = node;
			x._isRed = node._isRed;
			node._isRed = true;
			node.UpdateMax ();
			return x;
		}

		private Interval<N, T> RotateRight (Interval<N, T> node)
		{
			var x = node._left;
			node._left = x._right;
			x._right = node;
			x._isRed = node._isRed;
			node._isRed = true;
			node.UpdateMax ();
			return x;
		}

		private void FlipColors (Interval<N, T> node)
		{
			node._isRed = !node._isRed;
			node._left._isRed = !node._left._isRed;
			node._right._isRed = !node._right._isRed;
		}

		private Interval<N, T> MoveRedLeft (Interval<N, T> node)
		{
			FlipColors (node);
			if (IsRed (node._right._left))
			{
				node._right = RotateRight (node._right);
				node = RotateLeft (node);
				FlipColors (node);
			}
			return node;
		}

		private Interval<N, T> MoveRedRight (Interval<N, T> node)
		{
			FlipColors (node);
			if (IsRed (node._left._left))
			{
				node = RotateRight (node);
				FlipColors (node);
			}
			return node;
		}

		private Interval<N, T> Fixup (Interval<N, T> node)
		{
			// Rebalance if RB-invariants are broken.
			if (IsRed (node._right) && !IsRed (node._left))
				node = RotateLeft (node);
			if (IsRed (node._left) && IsRed (node._left._left))
				node = RotateRight (node);
			if (IsRed (node._left) && IsRed (node._right))
				FlipColors (node);
			// Update max high value for subtree.
			node.UpdateMax ();
			return node;
		}

		public Interval<N, T> Add (N low, N high, T data)
		{
			return Add (new Interval<N, T> (low, high, data));
		}

		public Interval<N, T> Add (Interval<N, T> interval)
		{
			if (interval == null)
				throw new ArgumentNullException ("interval");
			Interval<N, T> found = null;
			_root = AddNode (_root, interval, ref found);
			_root._isRed = false;
			_count++;
			_version++;
			return found ?? interval;
		}

		private Interval<N, T> AddNode (Interval<N, T> node, Interval<N, T> newNode, ref Interval<N, T> found)
		{
			if (node == null)
				return newNode;
			var cmp = newNode.Low.CompareTo (node.Low);
			if (cmp == 0 && newNode.High.CompareTo (node.High) == 0)
			{
				found = node;
				return node;
			}
			if (cmp < 0)
				node._left = AddNode (node._left, newNode, ref found);
			else
				node._right = AddNode (node._right, newNode, ref found);
			return found != null ? node : Fixup (node);
		}

		public int Remove (Interval<N, T> interval)
		{
			if (interval == null)
				throw new ArgumentNullException ("interval");
			_root = RemoveNode (_root, interval);
			if (_root != null)
				_root._isRed = false;
			_version++;
			return --_count;
		}

		private Interval<N, T> RemoveNode (Interval<N, T> current, Interval<N, T> node)
		{
			if (node.Low.CompareTo (current.Low) < 0)
			{
				if (current._left == null)
					throw new InvalidOperationException ("Interval not found in the tree.");
				if (!IsRed (current._left) && !IsRed (current._left._left))
					current = MoveRedLeft (current);
				current._left = RemoveNode (current._left, node);
			}
			else
			{
				if (IsRed (current._left))
					current = RotateRight (current);
				if (node == current && current._right == null)
					return null;
				if (current._right == null)
					throw new InvalidOperationException ("Interval not found in the tree.");
				if (!IsRed (current._right) && !IsRed (current._right._left))
					current = MoveRedRight (current);
				if (node == current)
				{
					var succ = current._right;
					while (succ._left != null)
						succ = succ._left;
					current = current.AssignFrom (succ);
					current._right = RemoveNode (current._right, succ);
				}
				else current._right = RemoveNode (current._right, node);
			}
			return Fixup (current);
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
			return _count == 0 ? "[ ]" :
				"[ " + this.Select (ival => ival.ToString ()).Aggregate ((s1, s2) => s1 + ", " + s2) + " ]";
		}
		
		#region IVisualizable implementation

		private Visual NodeVisual (string text, Color color, Visual parent)
		{
			var node = Visual.Frame (Visual.Margin (Visual.Label (text), 2, 2, 2, 2), FrameKind.RoundRectangle);
			return Visual.Anchor (
				parent == null ? 
					node :
					Visual.Styled (Visual.Connector (node, parent, HAlign.Center, VAlign.Top),
						new VisualStyle (pen: new Pen (color, 1))),
				HAlign.Center, VAlign.Bottom);
		}

		private Visual TreeVisual (Interval<N, T> interval, Visual parent)
		{
			if (interval == null)
				return Visual.Margin (NodeVisual ("-", Color.Black, parent), right: 4, bottom: 4);
			var node = NodeVisual (interval.ToString (), interval._isRed ? Color.Red : Color.Black, parent);
			return Visual.VStack (HAlign.Center, Visual.Margin (node, right: 4, bottom: 20),
				Visual.HStack (VAlign.Top, TreeVisual (interval._left, node), TreeVisual (interval._right, node)));
		}	

		public Visual ToVisual ()
		{
			return TreeVisual (_root, null);
		}

		#endregion
	}
}
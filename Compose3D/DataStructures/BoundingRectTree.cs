namespace Compose3D.DataStructures
{
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using Maths;

	public class BoundingRectTree<T> : IBoundingTree<Vec2, T>
	{
		private IntervalTree<float, IntervalTree<float, Seq<T>>> _tree;
		private int _count;

		public BoundingRectTree ()
		{
			_tree = new IntervalTree<float, IntervalTree<float, Seq<T>>> ();
		}

		public void Add (Aabb<Vec2> rect, T value)
		{
			var xival = _tree.Add (rect.Left, rect.Right, null);
			if (xival.Data == null)
				xival.Data = new IntervalTree<float, Seq<T>> ();
			var yival = xival.Data.Add (rect.Bottom, rect.Top, null);
			yival.Data = Seq.Cons (value, yival.Data);
			_count++;
		}

		public IEnumerable<KeyValuePair<Aabb<Vec2>, T>> Overlap (Aabb<Vec2> rect)
		{
			return from xival in _tree.Overlap (rect.Left, rect.Right)
				   from yival in xival.Data.Overlap (rect.Bottom, rect.Top)
				   let box = new Aabb<Vec2> (new Vec2 (xival.Low, yival.Low), new Vec2 (xival.High, yival.High))
				   from data in yival.Data
				   select new KeyValuePair<Aabb<Vec2>, T> (box, data);
		}

		public void Remove (Aabb<Vec2> rect, T value)
		{
			var xival = _tree.Find (rect.Left, rect.Right);
			if (xival == null)
				throw new KeyNotFoundException ("Given rectangle not found in the tree");
			var yival = xival.Data.Find (rect.Bottom, rect.Top);
			if (yival == null)
				throw new KeyNotFoundException ("Given rectangle not found in the tree");
			var newData = yival.Data.Remove (value);
			if (yival.Data == newData)
				throw new KeyNotFoundException ("Given value was not found");
			yival.Data = newData;
			_count--;
			if (newData != null)
				return;
			if (xival.Data.Count > 1)
				xival.Data.Remove (yival);
			else 
				_tree.Remove (xival);
		}

		public int Count
		{
			get { return _count; }
		}

		public IEnumerator<KeyValuePair<Aabb<Vec2>, T>> GetEnumerator ()
		{
			return (from xival in _tree
					from yival in xival.Data
					let box = new Aabb<Vec2> (new Vec2 (xival.Low, yival.Low), new Vec2 (xival.High, yival.High))
					from data in yival.Data
					select new KeyValuePair<Aabb<Vec2>, T> (box, data)).GetEnumerator ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		public override string ToString ()
		{
			return _tree.ToString ();
		}
	}
}
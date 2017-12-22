namespace ComposeFX.DataStructures
{
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using Maths;
	using System;

	public class BoundingBoxTree<T> : IBoundingTree<Vec3, T>
	{
		private IntervalTree<float, IntervalTree<float, IntervalTree<float, Seq<T>>>> _tree;
		private int _count;

		public BoundingBoxTree ()
		{
			_tree = new IntervalTree<float, IntervalTree<float, IntervalTree<float, Seq<T>>>> ();
		}

		public void Add (Aabb<Vec3> rect, T value)
		{
			var xival = _tree.Add (rect.Left, rect.Right, null);
			if (xival.Data == null)
				xival.Data = new IntervalTree<float, IntervalTree<float, Seq<T>>> ();
			var yival = xival.Data.Add (rect.Bottom, rect.Top, null);
			if (yival.Data == null)
				yival.Data = new IntervalTree<float, Seq<T>> ();
			var zival = yival.Data.Add (rect.Back, rect.Front, null);
			if (zival.Data.Contains (value))
				throw new InvalidOperationException ("Same value added twice.");
			zival.Data = Seq.Cons (value, zival.Data);
			_count++;
		}

		public IEnumerable<KeyValuePair<Aabb<Vec3>, T>> Overlap (Aabb<Vec3> rect)
		{
			return from xival in _tree.Overlap (rect.Left, rect.Right)
				   from yival in xival.Data.Overlap (rect.Bottom, rect.Top)
				   from zival in yival.Data.Overlap (rect.Back, rect.Front)
				   let box = new Aabb<Vec3> (new Vec3 (xival.Low, yival.Low, zival.Low), 
								 new Vec3 (xival.High, yival.High, zival.High))
				   from data in zival.Data
				   select new KeyValuePair<Aabb<Vec3>, T> (box, data);
		}

		public void Remove (Aabb<Vec3> box, T value)
		{
			var xival = _tree.Find (box.Left, box.Right);
			if (xival == null)
				throw new KeyNotFoundException ("Given box not found in the tree");
			var yival = xival.Data.Find (box.Bottom, box.Top);
			if (yival == null)
				throw new KeyNotFoundException ("Given box not found in the tree");
			var zival = yival.Data.Find (box.Back, box.Front);
			if (zival == null)
				throw new KeyNotFoundException ("Given box not found in the tree");
			var newData = zival.Data.Remove (value);
			if (newData == zival.Data)
				throw new KeyNotFoundException ("Given value was not found");
			zival.Data = newData;
			_count--;
			if (newData != null)
				return;
			if (xival.Data.Count > 1)
			{
				if (yival.Data.Count > 1)
					yival.Data.Remove (zival);
				else
					xival.Data.Remove (yival);
			}
			else
				_tree.Remove (xival);
		}

		public int Count
		{
			get { return _count; }
		}

		public IEnumerator<KeyValuePair<Aabb<Vec3>, T>> GetEnumerator ()
		{
			return (from xival in _tree
					from yival in xival.Data
					from zival in yival.Data
					let box = new Aabb<Vec3> (new Vec3 (xival.Low, yival.Low, zival.Low), 
						new Vec3 (xival.High, yival.High, zival.High))
					from data in zival.Data
					select new KeyValuePair<Aabb<Vec3>, T> (box, data)).GetEnumerator ();
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

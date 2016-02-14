namespace Compose3D.DataStructures
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Maths;

	public class RectangleTree<T>
	{
		private IntervalTree<float, IntervalTree<float, T>> _tree;

		public RectangleTree ()
		{
			_tree = new IntervalTree<float, IntervalTree<float, T>> ();
		}

		public void Add (Aabb<Vec2> rect, T data)
		{
			var interval = _tree.Add (rect.Left, rect.Right, new IntervalTree<float, T> ());
			interval.Data.Add (rect.Bottom, rect.Top, data);
		}
	}
}

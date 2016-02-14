namespace Compose3D.DataStructures
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	public class RectangleTree<N, T>  where N : struct, IComparable<N>
	{
		private IntervalTree<N, IntervalTree<N, T>> _nestedTree;

		public RectangleTree ()
		{
			_nestedTree = new IntervalTree<N, IntervalTree<N, T>> ();
		}

	}
}

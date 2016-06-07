namespace Visuals
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Windows.Forms;

	public class MouseRegions<T> where T : class
	{
		private List<Tuple<RectangleF, T>> _regions = new List<Tuple<RectangleF, T>> ();

		public void Clear ()
		{
			_regions.Clear ();
		}

		public Action<RectangleF> Add (T item)
		{
			return rect => _regions.Add (Tuple.Create (rect, item));
		}

		public T ItemUnderMouse (PointF mouseCoords)
		{
			foreach (var region in _regions)
			{
				if (region.Item1.Contains (mouseCoords.X, mouseCoords.Y))
					return region.Item2;
			}
			return null;
		}
	}
}

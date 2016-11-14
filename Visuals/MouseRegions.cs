namespace Visuals
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;

	public class MouseRegions<T>
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

		public Tuple<RectangleF, T> ItemUnderMouse (PointF mouseCoords)
		{
			foreach (var region in _regions)
			{
				if (region.Item1.Contains (mouseCoords.X, mouseCoords.Y))
					return region;
			}
			return null;
		}

		public int Count
		{
			get { return _regions.Count; }
		}

		public Tuple<RectangleF, T> this[int index]
		{
			get { return _regions[index]; }
		}
	}
}

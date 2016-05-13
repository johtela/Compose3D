namespace Visuals
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Windows.Forms;

	public enum MouseEventType { Move, Down, Up, Click, Wheel }

	public class MouseRegions
	{
		private List<Tuple<RectangleF, Action<MouseEventType, MouseEventArgs>>> _regions = 
			new List<Tuple<RectangleF, Action<MouseEventType, MouseEventArgs>>> ();

		public void Clear ()
		{
			_regions.Clear ();
		}

		public Action<RectangleF> Add (Action<MouseEventType, MouseEventArgs> action)
		{
			return rect => _regions.Add (Tuple.Create (rect, action));
		}

		public void TriggerEvent (MouseEventType type, MouseEventArgs args)
		{
			foreach (var region in _regions)
			{
				if (region.Item1.Contains (args.X, args.Y))
				{
					region.Item2 (type, args);
					return;
				}
			}
		}
	}
}

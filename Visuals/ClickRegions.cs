namespace Visuals
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	public class ClickRegions
	{
		private List<Tuple<RectangleF, Action>> _regions = 
			new List<Tuple<RectangleF, Action>> ();

		public void Clear ()
		{
			_regions.Clear ();
		}

		Action<RectangleF> Add (Action action)
		{
			return rect => _regions.Add (Tuple.Create (rect, action));
		}
	}
}

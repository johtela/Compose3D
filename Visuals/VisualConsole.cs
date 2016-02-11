namespace Visuals
{
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Windows.Forms;
	using System.Threading;

	public class VisualConsole : Form
	{
		private List<Visual> _visuals = new List<Visual> ();
		private VisualControl _control;
	  
		public VisualConsole ()
		{
			Parent = null;
			Text = "Visual Console";
			Size = new Size (700, 500);
			AutoScroll = true;
			BackColor = Color.White;
			_control = new VisualControl ();
			_control.Location = new Point (0, 0);
			_control.Parent = this;
			_control.Focus ();
		}

		public void ShowVisual (Visual visual)
		{
			lock (_visuals)
			{
				_visuals.Add (visual);
				_control.Visual = Visual.VStack (HAlign.Left,
					_visuals.SelectMany (v => new Visual[] { v, Visual.HRuler () }));
				Invalidate ();
			}
		}
	}
}

namespace Visuals
{
	using System;
	using System.Drawing;
	using System.Drawing.Drawing2D;
	using System.Drawing.Text;
	using System.Windows.Forms;

	public class VisualControl : Control
	{
		private Visual _visual;
		private VBox _size;

		public VisualControl ()
		{
			BackColor = Color.White;
			DoubleBuffered = true;
		}

		public Visual Visual
		{
			get { return _visual; }
			set
			{
				_visual = value;
				this.BeginInvoke (new Action (CalculateNewSize));
			}
		}

		private void CalculateNewSize ()
		{
			if (_visual != null)
			{
				_size = _visual.GetSize (new GraphicsContext (Graphics.FromHwnd (Handle)));
				Width = Convert.ToInt32 (_size.Width);
				Height = Convert.ToInt32 (_size.Height);
			}
			Invalidate ();
		}

		protected override void OnPaint (PaintEventArgs pe)
		{
			base.OnPaint (pe);

			if (_visual != null)
			{
				pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
				var ctx = new GraphicsContext (pe.Graphics, VisualStyle.Default);
				_visual.Render (ctx, _size);
			}
		}
	}
}

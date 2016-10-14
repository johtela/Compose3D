namespace Visuals
{
	using System.Drawing;
	using System.Drawing.Drawing2D;

	/// <summary>
	/// A style object that contains the brushes, pens, and fonts
	/// used for drawing visuals.
	/// </summary>
	public class VisualStyle
	{
		private readonly VisualStyle _parent;
		private readonly Font _font;
		private readonly Brush _textBrush;
		private readonly Pen _pen;
		private readonly Brush _brush;

		public static VisualStyle Default = new VisualStyle (
			font: new Font ("Arial", 10),
			textBrush: Brushes.Black,
			pen: new Pen (Color.Black, 1) { DashStyle = DashStyle.Dash },
			brush: Brushes.Gray);

		public VisualStyle (VisualStyle parent = null, Font font = null, 
			Brush textBrush = null, Pen pen = null, Brush brush = null)
		{
			_parent = parent ?? Default;
			_font = font;
			_textBrush = textBrush;
			_pen = pen;
			_brush = brush;
		}

		public VisualStyle Parent
		{
			get { return _parent; }
		}

		public Font Font
		{
			get
			{
				var vs = this;
				while (vs._font == null)
					vs = vs._parent;
				return vs._font;
			}
		}

		public Brush TextBrush
		{
			get
			{
				var vs = this;
				while (vs._textBrush == null)
					vs = vs._parent;
				return vs._textBrush;
			}
		}

		public Pen Pen
		{
			get
			{
				var vs = this;
				while (vs._pen == null)
					vs = vs._parent;
				return vs._pen;
			}
		}

		public Brush Brush
		{
			get
			{
				var vs = this;
				while (vs._brush == null)
					vs = vs._parent;
				return vs._brush;
			}
		}
	}

	public static class StyleHelpers
	{
		public static VisualStyle WithFontStyle (this VisualStyle style, FontStyle fontStyle)
		{
			return new VisualStyle (style, new Font (style.Font, fontStyle));
		}
	}
}

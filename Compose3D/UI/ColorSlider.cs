namespace Compose3D.UI
{
	using System.Drawing;
	using System.Drawing.Drawing2D;
	using System.Linq;
	using OpenTK.Input;
	using Extensions;
	using Maths;
	using Reactive;
	using Visuals;

	public class ColorSlider : Slider
	{
		public Color[] Colors { get; set; }

		public ColorSlider (VisualDirection direction, float knobWidth, float minVisualLength, 
			float value, Color[] colors, Reaction<float> changed)
			: base (direction, knobWidth, minVisualLength, 0f, 1f, value, changed)
		{
			Colors = colors;
		}

		private void DrawBar (GraphicsContext context, LinearGradientBrush brush, RectangleF rect)
		{
			var blend = new ColorBlend (Colors.Length);
			var step = 1f / (Colors.Length - 1);
			for (int i = 0; i < Colors.Length; i++)
			{
				blend.Colors [i] = Colors [i];
				blend.Positions [i] = i * step;
			}
			brush.InterpolationColors = blend;
			context.Graphics.FillRectangle (brush, rect);
		}

		protected override void PaintHorizontalBar (GraphicsContext context, SizeF size, SizeF knobSize)
		{
			var rect = new RectangleF (new PointF (), new SizeF (size.Width - knobSize.Width, knobSize.Height));
			var brush = new LinearGradientBrush (rect, Color.Black, Color.Black,
				LinearGradientMode.Horizontal);
			DrawBar (context, brush, rect);
		}

		protected override void PaintVerticalBar (GraphicsContext context, SizeF size, SizeF knobSize)
		{
			var rect = new RectangleF (new PointF (), new SizeF (knobSize.Width, size.Height - knobSize.Height));
			var brush = new LinearGradientBrush (rect, Color.Black, Color.Black,
				LinearGradientMode.Vertical);
			DrawBar (context, brush, rect);
		}

		public static ColorSlider Hue (VisualDirection direction, float knobWidth, float minVisualLength, 
			float hue, Reaction<float> changed)
		{
			return new ColorSlider (direction, knobWidth, minVisualLength, hue, 
				new [] { 
					Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Blue, Color.Indigo, 
					Color.Violet, Color.Red
				}, changed);
		}

		public static ColorSlider Saturation (VisualDirection direction, float knobWidth, float minVisualLength, 
			float hue, float saturation, Reaction<float> changed)
		{
			return new ColorSlider (direction, knobWidth, minVisualLength, saturation, 
				new [] { Color.White, VisualHelpers.ColorFromHSB (hue, 1f, 1f) }, changed);
		}
	}
}


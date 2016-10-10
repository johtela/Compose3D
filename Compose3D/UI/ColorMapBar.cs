namespace Compose3D.UI
{
	using System;
	using System.Drawing;
	using System.Drawing.Drawing2D;
	using OpenTK.Input;
	using Maths;
	using Imaging;
	using Reactive;
	using Visuals;

	public class ColorMapBar : Control
	{
		public readonly Reaction<ColorMap<Vec3>> Changed;
		public readonly Reaction<Tuple<float, Color>> ItemSelected;
		public readonly float DomainMin;
		public readonly float DomainMax;
		public readonly SizeF MinSize;
		public readonly ColorMap<Vec3> Value;

		// Click regions
		private MouseRegions<IVisualizable> _mouseRegions;

		// Control state
		private IVisualizable _pressed;
		private IVisualizable _highlighted;

		public ColorMapBar (float domainMin, float domainMax, SizeF minSize, 
			ColorMap<Vec3> value, Reaction<ColorMap<Vec3>> changed, 
			Reaction<Tuple<float, Color>> itemSelected)
		{
			if (value.Count > 0 && (value.MinKey < domainMin || value.MaxKey > domainMax))
				throw new ArgumentException ("Color map values out of min/max range.");
			DomainMin = domainMin;
			DomainMax = domainMax;
			MinSize = minSize;
			Value = value;
			Changed = changed;
			ItemSelected = itemSelected;
		}

		public override void HandleInput (PointF relativeMousePos)
		{
		}

		private SizeF Paint (GraphicsContext context, SizeF size)
		{
			var cnt = Value.Count;
			var blend = new ColorBlend (cnt + 2);
			var domain = DomainMax - DomainMin;
			blend.Colors[0] = Value[DomainMin].ToColor ();
			blend.Positions[0] = 0f;
			for (int i = 0; i < cnt; i++)
			{
				var val = Value.SamplePoints.Keys[i] - DomainMin;
				blend.Colors[i + 1] = Value.SamplePoints.Values[i].ToColor ();
				blend.Positions[i + 1] = val / domain;
			}
			blend.Colors [cnt + 1] = Value[DomainMax].ToColor ();
			blend.Positions[cnt + 1] = 1f;
			var rect = new RectangleF (new PointF (0f, 0f), new SizeF (size.Width / 2f, size.Height));
			var brush = new LinearGradientBrush (rect, Color.Black, Color.Black, LinearGradientMode.Vertical);
			brush.InterpolationColors = blend;
			context.Graphics.FillRectangle (brush, rect);
			return size;
		}

		public override Visual ToVisual ()
		{
			return Visual.Custom (MinSize, Paint);
		}
	}
}

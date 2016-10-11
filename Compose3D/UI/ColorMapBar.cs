namespace Compose3D.UI
{
	using System;
	using System.Drawing;
	using System.Drawing.Drawing2D;
	using System.Linq;
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

		private SizeF PaintBar (GraphicsContext context, SizeF size)
		{
			var cnt = Value.Count;
			var blend = new ColorBlend (cnt + 2);
			blend.Colors[0] = Value[DomainMin].ToColor ();
			blend.Positions[0] = 0f;
			var i = 1;
			foreach (var sp in Value.NormalizedSamplePoints (DomainMin, DomainMax))
			{
				blend.Colors[i] = sp.Value.ToColor ();
				blend.Positions[i++] = sp.Key;
			}
			blend.Colors [i] = Value[DomainMax].ToColor ();
			blend.Positions[i] = 1f;
			var rect = new RectangleF (new PointF (0f, 0f), new SizeF (size.Width, size.Height));
			var brush = new LinearGradientBrush (rect, Color.Black, Color.Black, LinearGradientMode.Vertical);
			brush.InterpolationColors = blend;
			context.Graphics.FillRectangle (brush, rect);
			return size;
		}

		private SizeF PaintKnob (Color color, GraphicsContext context, SizeF size)
		{
			context.Graphics.FillRectangle (new SolidBrush (color), 0f, 0f, size.Width, size.Height);
			context.Graphics.DrawRectangle (context.Style.Pen, 0f, 0f, size.Width, size.Height);
			return size;
		}

		public override Visual ToVisual ()
		{
			var knobSize = new SizeF (MinSize.Width / 2f, MinSize.Width / 4f);
			return Visual.Margin (
				Visual.HStack (VAlign.Top,
					Visual.Custom (new SizeF (MinSize.Width / 2f, MinSize.Height), PaintBar),
					Visual.VPile (HAlign.Left, VAlign.Top,
						Value.NormalizedSamplePoints (DomainMin, DomainMax).Select (sp => 
							Tuple.Create (sp.Key, Visual.Custom (knobSize, (ctx, size) => 
								PaintKnob (sp.Value.ToColor (), ctx, size)))))),
				top: knobSize.Height, bottom: knobSize.Height);
		}
	}
}

namespace Compose3D.UI
{
	using System.Drawing;
	using System.Drawing.Drawing2D;
	using OpenTK.Input;
	using Maths;
	using Reactive;
	using Visuals;

	public class Slider : Control
	{
		public readonly VisualDirection Direction;
		public readonly float KnobWidth;
		public readonly float MinVisualLength;
		public readonly float MinValue;
		public readonly float Range;
		public readonly Reaction<float> Changed;

		public float Value { get; set; }

		// Click region
		private RectangleF _clickRegion;

		public Slider (VisualDirection direction, float knobWidth, float minVisualLength, 
			float minValue, float maxValue, float value, Reaction<float> changed)
		{
			Direction = direction;
			KnobWidth = knobWidth;
			MinVisualLength = minVisualLength;
			MinValue = minValue;
			Range = maxValue - minValue;
			Value = value;
			Changed = changed;
		}

		public override void HandleInput (PointF relativeMousePos)
		{
			if (MouseButtonDown (MouseButton.Left) && _clickRegion.Contains (relativeMousePos))
			{
				var pos = relativeMousePos - new SizeF (_clickRegion.Location);
				var relPos = (Direction == VisualDirection.Horizontal ?
					(pos.X  - (KnobWidth / 4f)) / (_clickRegion.Width - KnobWidth) :
					1f - ((pos.Y - (KnobWidth / 4f )) / (_clickRegion.Height - KnobWidth)))
					.Clamp (0f, 1f);
				var newVal = MinValue + (relPos * Range);
				if (newVal != Value)
				{
					Value = newVal;
					Changed (Value);
				}
			}
		}

		private Pen BarPen (GraphicsContext context)
		{
			var pen = new Pen (context.Style.Brush, KnobWidth / 2f);
			pen.StartCap = LineCap.Round;
			pen.EndCap = LineCap.Round;
			return pen;
		}

		protected virtual void PaintHorizontalBar (GraphicsContext context, SizeF size, SizeF knobSize)
		{
			var Y = knobSize.Height / 2f;
			context.Graphics.DrawLine (BarPen (context), knobSize.Width, Y, size.Width - (knobSize.Width * 2f), Y);
		}

		protected virtual void PaintVerticalBar (GraphicsContext context, SizeF size, SizeF knobSize)
		{
			var X = knobSize.Width / 2f;
			context.Graphics.DrawLine (BarPen (context), X, knobSize.Height, X, size.Height - (knobSize.Height * 2f));
		}

		private SizeF Paint (GraphicsContext context, SizeF size)
		{
			PointF pos;
			SizeF knobSize;
			var ratio = (Value - MinValue) / Range;
			if (Direction == VisualDirection.Horizontal)
			{
				knobSize = new SizeF (KnobWidth / 2f, KnobWidth);
				pos = new PointF (ratio * (size.Width - KnobWidth), 0f);
				PaintHorizontalBar (context, size, knobSize);
			}
			else
			{
				knobSize = new SizeF (KnobWidth, KnobWidth / 2f);
				pos = new PointF (0f, (1f - ratio) * (size.Height - KnobWidth)); 
				PaintVerticalBar (context, size, knobSize);
			}
			context.Graphics.FillRectangle (context.Style.Brush, pos.X, pos.Y, knobSize.Width, knobSize.Height);
			context.Graphics.DrawRectangle (context.Style.Pen, pos.X, pos.Y, knobSize.Width, knobSize.Height);
			return Direction == VisualDirection.Horizontal ?
				new SizeF (size.Width, KnobWidth) : 
				new SizeF (KnobWidth, size.Height);
		}

		public override Visual ToVisual (SizeF panelSize)
		{
			var size = Direction == VisualDirection.Horizontal ?
				new SizeF (MinVisualLength, KnobWidth) : 
				new SizeF (KnobWidth, MinVisualLength);
			return Visual.Clickable (Visual.Custom (size, Paint), rect => _clickRegion = rect);
		}
	}
}

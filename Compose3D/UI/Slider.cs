namespace Compose3D.UI
{
	using System.Drawing;
	using System.Drawing.Drawing2D;
	using System.Linq;
	using System.Globalization;
	using OpenTK.Input;
	using Reactive;
	using Visuals;
	using System;

	public class Slider : Control
	{
		public readonly VisualDirection Direction;
		public readonly float KnobWidth;
		public readonly float MinValue;
		public readonly float MaxValue;

		public float Value { get; private set; }

		public Slider (VisualDirection direction, float knobWidth, float minValue, float maxValue, float value)
		{
			Direction = direction;
			KnobWidth = knobWidth;
			MinValue = minValue;
			MaxValue = maxValue;
			Value = value;
		}

		public override void HandleInput (PointF relativeMousePos)
		{
		}

		private void DrawKnob (GraphicsContext context, SizeF size)
		{
			PointF pos;
			SizeF knobSize;
			var ratio = (Value - MinValue) / (MaxValue - MinValue);
			var pen = new Pen (context.Style.Brush, KnobWidth / 2f);
			pen.StartCap = LineCap.Round;
			pen.EndCap = LineCap.Round;
			if (Direction == VisualDirection.Horizontal)
			{
				knobSize = new SizeF (KnobWidth / 2f, KnobWidth);
				pos = new PointF (ratio * (size.Width - KnobWidth), 0f); 
				var Y = knobSize.Height / 2f;
				context.Graphics.DrawLine (pen, knobSize.Width, Y, size.Width - knobSize.Width, Y);
			}
			else
			{
				knobSize = new SizeF (KnobWidth, KnobWidth / 2f);
				pos = new PointF (0f, (1f - ratio) * (size.Height - KnobWidth)); 
				var X = knobSize.Width / 2f;
				context.Graphics.DrawLine (pen, X, knobSize.Height, X, size.Height - knobSize.Height);
			}
			context.Graphics.FillRectangle (context.Style.Brush, pos.X, pos.Y, knobSize.Width, knobSize.Height);
			context.Graphics.DrawRectangle (context.Style.Pen, pos.X, pos.Y, knobSize.Width, knobSize.Height);
		}

		public override Visual ToVisual ()
		{
			var range = MaxValue - MinValue;
			var size = Direction == VisualDirection.Horizontal ?
				new SizeF (range, KnobWidth) : 
				new SizeF (KnobWidth, range);
			return Visual.Custom (size, DrawKnob);
		}
	}
}

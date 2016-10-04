﻿namespace Compose3D.UI
{
	using System.Drawing;
	using System.Drawing.Drawing2D;
	using OpenTK.Input;
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

		public float Value { get; private set; }

		// Click region
		private RectangleF _clickRegion;

		public Slider (VisualDirection direction, float knobWidth, float minVisualLength, 
			float minValue, float maxValue, float value, Reaction<float> changed)
		{
			Direction = direction;
			KnobWidth = knobWidth;
			MinVisualLength = minVisualLength;
			MinValue = minValue;
			Range = maxValue - minValue + 1;
			Value = value;
			Changed = changed;
		}

		public override void HandleInput (PointF relativeMousePos)
		{
			if (MouseButtonDown (MouseButton.Left) && _clickRegion.Contains (relativeMousePos))
			{
				var pos = relativeMousePos - new SizeF (_clickRegion.Location);
				var relPos = Direction == VisualDirection.Horizontal ?
					pos.X / _clickRegion.Width :
					1f - (pos.Y / _clickRegion.Height);
				var newVal = MinValue + (relPos * Range);
				if (newVal != Value)
				{
					Value = newVal;
					Changed (Value);
				}
			}
		}

		private void Paint (GraphicsContext context, SizeF size)
		{
			PointF pos;
			SizeF knobSize;
			var ratio = (Value - MinValue) / Range;
			var pen = new Pen (context.Style.Brush, KnobWidth / 2f);
			pen.StartCap = LineCap.Round;
			pen.EndCap = LineCap.Round;
			if (Direction == VisualDirection.Horizontal)
			{
				knobSize = new SizeF (KnobWidth / 2f, KnobWidth);
				pos = new PointF (ratio * (size.Width - KnobWidth), 0f); 
				var Y = knobSize.Height / 2f;
				context.Graphics.DrawLine (pen, knobSize.Width, Y, size.Width - (knobSize.Width * 2f), Y);
			}
			else
			{
				knobSize = new SizeF (KnobWidth, KnobWidth / 2f);
				pos = new PointF (0f, (1f - ratio) * (size.Height - KnobWidth)); 
				var X = knobSize.Width / 2f;
				context.Graphics.DrawLine (pen, X, knobSize.Height, X, size.Height - (knobSize.Height * 2f));
			}
			context.Graphics.FillRectangle (context.Style.Brush, pos.X, pos.Y, knobSize.Width, knobSize.Height);
			context.Graphics.DrawRectangle (context.Style.Pen, pos.X, pos.Y, knobSize.Width, knobSize.Height);
		}

		public override Visual ToVisual ()
		{
			var size = Direction == VisualDirection.Horizontal ?
				new SizeF (MinVisualLength, KnobWidth) : 
				new SizeF (KnobWidth, MinVisualLength);
			return Visual.Clickable (Visual.Custom (size, Paint), rect => _clickRegion = rect);
		}
	}
}

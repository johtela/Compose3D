namespace Compose3D.UI
{
	using System.Drawing;
	using System.Linq;
	using System.Globalization;
	using OpenTK.Input;
	using Reactive;
	using Visuals;
	using System;

	public class Slider : Control
	{
		public readonly VisualDirection Direction;
		public readonly float Length;
		public readonly float MinValue;
		public readonly float MaxValue;

		public float Value { get; private set; }

		public Slider (VisualDirection direction, float length, float minValue, float maxValue, float value)
		{
			Direction = direction;
			Length = length;
			MinValue = minValue;
			MaxValue = maxValue;
			Value = value;
		}

		public override void HandleInput (PointF relativeMousePos)
		{
			throw new NotImplementedException ();
		}

		public override Visual ToVisual ()
		{
			throw new NotImplementedException ();
		}
	}
}

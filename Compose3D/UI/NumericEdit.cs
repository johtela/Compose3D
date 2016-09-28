namespace Compose3D.UI
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Globalization;
	using OpenTK.Input;
	using Reactive;
	using Visuals;

	class NumericEdit : Control
	{
		public readonly float Increment;
		public readonly Reaction<float> Changed;

		// Click region
		private RectangleF _clickRegion;

		// Control state
		private bool _active;
		private bool _pressed;
		private string _value;

		public float Value
		{
			get { return float.Parse (_value, CultureInfo.InvariantCulture); }
		}

		public NumericEdit (float value, float increment, Reaction<float> changed)
		{
			_value = value.ToString (CultureInfo.InvariantCulture);
			Increment = increment;
			Changed = changed;
		}

		public override void HandleInput (PointF relativeMousePos)
		{
		}

		public override Visual ToVisual ()
		{
			return Visual.Clickable (Visual.Frame (Visual.Label (_value), FrameKind.Rectangle), 
				rect => _clickRegion = rect);
		}
	}
}

﻿namespace Compose3D.UI
{
	using System;
	using System.Drawing;
	using System.Linq;
	using System.Globalization;
	using OpenTK.Input;
	using Maths;
	using Reactive;
	using Visuals;

	public class NumericEdit : Control
	{
		public readonly bool IsInteger;
		public readonly float Increment;
		public readonly Reaction<float> Changed;

		// Click region
		private RectangleF _clickRegion;

		// Control state
		private bool _active;
		private string _value;

		public float Value
		{
			get { return float.Parse (_value, CultureInfo.InvariantCulture); }
			set { _value = value.ToString (CultureInfo.InvariantCulture); }
		}

		public NumericEdit (float value, bool isInteger, float increment, Reaction<float> changed)
		{
			IsInteger = isInteger;
			Value = isInteger ? value.Round () : value;
			Increment = isInteger ? increment.Round () : increment;
			Changed = changed;
		}

		private void NotifyChanged ()
		{
			float x;
			if (float.TryParse (_value, NumberStyles.Any, CultureInfo.InvariantCulture, out x))
				Changed (x);
		}

		public override void HandleInput (PointF relativeMousePos)
		{
			if (_active)
			{
				var num = InputState.AnyNumberKeyPressed ();
				if (num >= '0' && num <= '9')
				{
					_value += num;
					NotifyChanged ();
				}
				else if (num == '-' && _value.Length == 0)
					_value = "-";
				else if (!IsInteger && num == '.' && _value.Length > 0 && !_value.Contains ('.'))
					_value += '.';
				else if (InputState.KeyPressed (Key.BackSpace, true) && _value.Length > 0)
				{
					_value = _value.Substring (0, _value.Length - 1);
					NotifyChanged ();
				}
				else if (InputState.KeyPressed (Key.Up, true))
				{
					Value = Value + Increment;
					NotifyChanged ();
				}
				else if (InputState.KeyPressed (Key.Down, true))
				{
					Value = Value - Increment;
					NotifyChanged ();
				}
				else if (InputState.MouseWheelChange () != 0)
				{
					Value = Value + (InputState.MouseWheelChange () * Increment);
					NotifyChanged ();
				}
			}
			if (InputState.MouseButtonPressed (MouseButton.Left))
				_active = _clickRegion.Contains (relativeMousePos);
		}

		public override Visual ToVisual (SizeF panelSize)
		{
			var visual = Visual.Clickable (
				Visual.Frame (
					Visual.Margin (Visual.Label (_value + (_active ? "_" : "")), left: 2f, right: 8f),
					FrameKind.Rectangle, _active), 
				rect => _clickRegion = rect);
			return _active ? Visual.Styled (visual, SelectedStyle) : visual;
		}
	}
}

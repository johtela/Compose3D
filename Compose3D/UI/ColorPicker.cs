namespace Compose3D.UI
{
	using System;
	using System.Drawing;
	using System.Linq;
	using Reactive;
	using Visuals;
	using Extensions;

	public class ColorPicker : Container
	{
		private ColorSlider _hue;
		private ColorSlider _saturation;
		private ColorSlider _brightness;
		private Color _value;

		public readonly Reaction<Color> Changed;
		public Color Value
		{
			get { return _value; }
			set
			{
				_value = value;
				var hsb = value.ToHSB ();
				_hue.Value = hsb.Item1;
				_saturation.Value = hsb.Item2;
				_brightness.Value = hsb.Item3;
				int i = LastColorIndex ();
				_saturation.Colors[i] = VisualHelpers.ColorFromHSB (hsb.Item1, 1f, 1f);
				_brightness.Colors[i] = VisualHelpers.ColorFromHSB (hsb.Item1, _saturation.Value, 1f);
			}
		}

		public ColorPicker (VisualDirection direction, float knobWidth, float minVisualLength, Color color,
			bool preview, Reaction<Color> changed)
			: base (preview ? direction : direction.Opposite (), HAlign.Left, VAlign.Top, true, false, 
				Enumerable.Empty<Control> ())
		{
			Changed = changed;
			_hue = ColorSlider.Hue (direction, knobWidth, minVisualLength, color, 
				React.By<float> (ChangeHue));
			_saturation = ColorSlider.Saturation (direction, knobWidth, minVisualLength, color, 
				React.By<float> (ChangeSaturation));
			_brightness = ColorSlider.Brightness (direction, knobWidth, minVisualLength, color, 
				React.By<float> (ChangeBrightness));
			var controls = EnumerableExt.Enumerate<Control> (_hue, _saturation, _brightness);
			var contents = preview ?
				EnumerableExt.Enumerate (
	               new Container (direction, HAlign.Center, VAlign.Center, false, false,
		               new Container (direction.Opposite (), HAlign.Left, VAlign.Top, false, false, controls),
		               Label.ColorPreview (() => _value, new SizeF (3.5f * knobWidth, 3.5f * knobWidth)))) :
				controls;
			Controls.AddRange (contents.Select (c => new Tuple<Control, Reaction<Control>> (c, null)));
		}

		private void UpdateValue ()
		{
			_value = VisualHelpers.ColorFromHSB (_hue.Value, _saturation.Value, _brightness.Value);
			Changed (_value);
		}

		private int LastColorIndex ()
		{
			return Direction == VisualDirection.Horizontal ? 1 : 0;
		}

		private void ChangeHue (float hue)
		{
			int i = LastColorIndex ();
			_saturation.Colors[i] = VisualHelpers.ColorFromHSB (hue, 1f, 1f);
			_brightness.Colors[i] = VisualHelpers.ColorFromHSB (hue, _saturation.Value, 1f);
			UpdateValue ();
		}

		private void ChangeSaturation (float saturation)
		{
			_brightness.Colors[LastColorIndex ()] = 
				VisualHelpers.ColorFromHSB (_hue.Value, _saturation.Value, 1f);
			UpdateValue ();
		}

		private void ChangeBrightness (float brightness)
		{
			_brightness.Colors[LastColorIndex ()] = 
				VisualHelpers.ColorFromHSB (_hue.Value, _saturation.Value, 1f);
			UpdateValue ();
		}
	}
}
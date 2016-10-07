namespace Compose3D.UI
{
	using System.Drawing;
	using Reactive;
	using Visuals;

	public class ColorPicker : Container
	{
		private ColorSlider _hue;
		private ColorSlider _saturation;
		private ColorSlider _brightness;

		public readonly Reaction<Color> Changed;
		public Color Value { get; set; }

		public ColorPicker (VisualDirection direction, float knobWidth, float minVisualLength, Color color,
			bool preview, Reaction<Color> changed)
			: base (preview ? direction : direction.Opposite (), HAlign.Left, VAlign.Top, true)
		{
			Changed = changed;
			_hue = ColorSlider.Hue (direction, knobWidth, minVisualLength, color, 
				React.By<float> (ChangeHue));
			_saturation = ColorSlider.Saturation (direction, knobWidth, minVisualLength, color, 
				React.By<float> (ChangeSaturation));
			_brightness = ColorSlider.Brightness (direction, knobWidth, minVisualLength, color, 
				React.By<float> (ChangeBrightness));
			var controls = new Control[] { _hue, _saturation, _brightness };
			Controls = preview ?
				new Control[]
				{
					new Container (direction, HAlign.Center, VAlign.Center, false, 
						new Container (direction.Opposite (), HAlign.Left, VAlign.Top, false, controls),
						Label.ColorPreview (() => Value, new SizeF (3.5f * knobWidth, 3.5f * knobWidth)))
				} :
				controls;
		}

		private void UpdateValue ()
		{
			Value = VisualHelpers.ColorFromHSB (_hue.Value, _saturation.Value, _brightness.Value);
			Changed (Value);
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
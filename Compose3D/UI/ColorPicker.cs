namespace Compose3D.UI
{
	using System.Drawing;
	using System.Drawing.Drawing2D;
	using System.Linq;
	using Reactive;
	using Visuals;
	using Extensions;

	public class ColorPicker : Container
	{
		public readonly Reaction<Color> Changed;
		public readonly ColorSlider Hue;
		public readonly ColorSlider Saturation;
		public readonly ColorSlider Brightness;

		public Color Value { get; set; }

		public ColorPicker (VisualDirection direction, float knobWidth, float minVisualLength, Color color,
			Reaction<Color> changed)
			: base (direction.Opposite (), HAlign.Left, VAlign.Top) 
		{
			Changed = changed;
			Hue = ColorSlider.Hue (direction, knobWidth, minVisualLength, color, React.By<float> (ChangeHue));
			Saturation = ColorSlider.Saturation (direction, knobWidth, minVisualLength, color, React.By<float> (ChangeSaturation));
			Brightness = ColorSlider.Brightness (direction, knobWidth, minVisualLength, color, React.By<float> (ChangeBrightness));
			Controls = new ColorSlider [] { Hue, Saturation, Brightness }; 
		}

		private void ChangeHue (float hue)
		{
			Saturation.Colors[1] = VisualHelpers.ColorFromHSB (hue, 1f, 1f);
			Brightness.Colors[1] = VisualHelpers.ColorFromHSB (hue, Saturation.Value, 1f);
		}

		private void ChangeSaturation (float saturation)
		{
			Brightness.Colors[1] = VisualHelpers.ColorFromHSB (Hue.Value, Saturation.Value, 1f);
		}

		private void ChangeBrightness (float brightness)
		{
			Brightness.Colors[1] = VisualHelpers.ColorFromHSB (Hue.Value, Saturation.Value, 1f);
		}
	}
}
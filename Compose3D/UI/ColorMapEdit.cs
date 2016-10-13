namespace Compose3D.UI
{
	using System;
	using System.Drawing;
	using Imaging;
	using Reactive;
	using Maths;
	using Visuals;

	public class ColorMapEdit : Container
	{
		private ColorMapBar _bar;
		private ColorPicker _picker;
		private int? _selected;

		public ColorMap<Vec3> ColorMap
		{
			get { return _bar.ColorMap; }
		}

		public ColorMapEdit (float domainMin, float domainMax, float knobWidth, float minVisualLength,
			ColorMap<Vec3> colorMap, Reaction<ColorMap<Vec3>> changed)
			: base (VisualDirection.Horizontal, HAlign.Left, VAlign.Top, false)
		{
			_bar = new ColorMapBar (domainMin, domainMax, knobWidth, minVisualLength, colorMap, changed, 
				React.By<int?> (ItemSelected));
			_picker = new ColorPicker (VisualDirection.Vertical, knobWidth, minVisualLength - (3f * knobWidth), 
				Color.Black, true, React.By<Color> (ColorChanged));
			Controls = new Control[] { _bar, _picker };
		}

		private void ItemSelected (int? item)
		{
			if (item != null)
			{
				_selected = item;
				_picker.Value = _bar.ColorMap[item.Value].ToColor ();
			}
			else
				_selected = null;
		}

		private void ColorChanged (Color color)
		{
			if (_selected.HasValue)
			{
				_bar.ColorMap[_selected.Value] = color.ToVec3 ();
				_bar.Changed (_bar.ColorMap);
			}
		}
	}
}

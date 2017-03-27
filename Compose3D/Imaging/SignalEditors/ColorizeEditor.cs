namespace Compose3D.Imaging.SignalEditors
{
	using System.Collections.Generic;
	using System.Xml.Linq;
	using Extensions;
	using Imaging;
	using Visuals;
	using Reactive;
	using Maths;
	using Textures;
	using UI;

	internal class ColorizeEditor : SignalEditor<Vec3>
	{
		public SignalEditor<float> Source;
		public ColorMap<Vec3> ColorMap;

		public ColorizeEditor (Texture texture)
			: base (texture) { }

		protected override Control CreateControl ()
		{
			var changed = Changed.Adapt<ColorMap<Vec3>, AnySignalEditor> (this);
			return FoldableContainer.WithLabel ("Color Map", true, HAlign.Left,
				InputSignalControl ("Source", Source),
				new ColorMapEdit (-1f, 1f, 20f, 200f, ColorMap, changed));
		}

		protected override void Load (XElement xelem)
		{
			ColorMap = xelem.LoadColorMap<Vec3> ();
		}

		protected override void Save (XElement xelem)
		{
			xelem.SaveColorMap<Vec3> (ColorMap);
		}

		public override IEnumerable<AnySignalEditor> Inputs
		{
			get { return EnumerableExt.Enumerate (Source); }
		}

		public override Signal<Vec2, Vec3> Signal
		{
			get { return Source.Signal.Colorize (ColorMap); }
		}

        public ColorizeArgs<Vec3> Args
        {
            get { return new ColorizeArgs<Vec3> (ColorMap); }
        }
	}
}

namespace Compose3D.Imaging.SignalEditors
{
	using System.Collections.Generic;
	using System.Xml.Linq;
	using Extensions;
	using Imaging;
	using Visuals;
	using Reactive;
	using Maths;
	using UI;

	internal class MaskEditor : SignalEditor<float>
	{
		public SignalEditor<float> Source;
		public SignalEditor<float> Other;
		public SignalEditor<float> Mask;

		public override Signal<Vec2, float> Signal
		{
			get { return Source.Signal.Mask (Other.Signal, Mask.Signal); }
		}

		public override IEnumerable<AnySignalEditor> Inputs
		{
			get { return EnumerableExt.Enumerate (Source, Other, Mask); }
		}

		protected override Control CreateControl ()
		{
			return FoldableContainer.WithLabel ("Blend", true, HAlign.Left,
				InputSignalControl ("Source", Source),
				InputSignalControl ("Other", Other),
				InputSignalControl ("Mask", Mask));
		}

		protected override void Load (XElement xelem)
		{
		}

		protected override void Save (XElement xelem)
		{
		}
	}
}

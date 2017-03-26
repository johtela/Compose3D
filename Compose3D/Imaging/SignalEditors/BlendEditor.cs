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

	internal class BlendEditor : SignalEditor<float>
	{
		public float BlendFactor;
		public SignalEditor<float> Source;
		public SignalEditor<float> Other;

		public override Signal<Vec2, float> Signal
		{
			get { return Source.Signal.Blend (Other.Signal, BlendFactor); }
		}

		public override IEnumerable<AnySignalEditor> Inputs
		{
			get { return EnumerableExt.Enumerate (Source, Other); }
		}

		protected override Control CreateControl ()
		{
			var changed = Changed.Adapt<float, AnySignalEditor> (this);
			return FoldableContainer.WithLabel ("Blend", true, HAlign.Left,
				InputSignalControl ("Source", Source),
				InputSignalControl ("Other", Other),
				Container.LabelAndControl ("Blend Factor: ",
					new NumericEdit (BlendFactor, false, 0.1f,
						React.By ((float s) => BlendFactor = s).And (changed)), true));
		}

		protected override void Load (XElement xelem)
		{
			BlendFactor = xelem.AttrFloat (nameof (BlendFactor));
		}

		protected override void Save (XElement xelem)
		{
			xelem.SetAttributeValue (nameof (BlendFactor), BlendFactor);
		}
	}
}

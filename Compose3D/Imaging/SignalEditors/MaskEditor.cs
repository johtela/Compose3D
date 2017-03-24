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

	public class MaskEditor<V> : SignalEditor<V, float>
		where V : struct, IVec<V, float>
	{
		public SignalEditor<V, float> Source;
		public SignalEditor<V, float> Other;
		public SignalEditor<V, float> Mask;

		public override Signal<V, float> Signal
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

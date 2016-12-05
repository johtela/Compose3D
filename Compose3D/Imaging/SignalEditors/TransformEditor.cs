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

	internal class TransformEditor<V> : SignalEditor<V, float>
		where V : struct, IVec<V, float>
	{
		public float Scale;
		public float Offset;
		public SignalEditor<V, float> Source;

		public override Signal<V, float> Signal
		{
			get { return Source.Signal.Scale (Scale).Offset (Offset).Clamp (-1f, 1f); }
		}

		public override IEnumerable<AnySignalEditor> Inputs
		{
			get { return EnumerableExt.Enumerate (Source); }
		}

		protected override Control CreateControl ()
		{
			var changed = Changed.Adapt<float, AnySignalEditor> (this);
			return FoldableContainer.WithLabel ("Transform", true, HAlign.Left,
				InputSignalControl ("Source", Source),
				Container.LabelAndControl ("Scale: ",
					new NumericEdit (Scale, false, 0.1f,
						React.By ((float s) => Scale = s).And (changed)), true),
				Container.LabelAndControl ("Offset: ",
					new NumericEdit (Offset, false, 0.1f,
						React.By ((float s) => Offset = s).And (changed)), true));
		}

		protected override void Load (XElement xelem)
		{
			Scale = xelem.AttrFloat (nameof (Scale));
			Offset = xelem.AttrFloat (nameof (Offset));
		}

		protected override void Save (XElement xelem)
		{
			xelem.SetAttributeValue (nameof (Scale), Scale);
			xelem.SetAttributeValue (nameof (Offset), Offset);
		}

		protected override string ToCode ()
		{
			return MethodSignature (Source.Name, "Transform", Name, Scale, Offset);
		}
	}
}

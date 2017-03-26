namespace Compose3D.Imaging.SignalEditors
{
	using System.Collections.Generic;
	using System.Xml.Linq;
	using Extensions;
	using CLTypes;
	using Imaging;
	using Visuals;
	using Reactive;
	using Maths;
	using UI;

	public class WarpEditor<V> : SignalEditor<V, float>
		where V : struct, IVec<V, float>
	{
		public float Scale;
		public V Dv;
		public SignalEditor<V, float> Source;
		public SignalEditor<V, float> Warp;

		protected override Control CreateControl ()
		{
			var changed = Changed.Adapt<float, AnySignalEditor> (this);
			return FoldableContainer.WithLabel ("Warp", true, HAlign.Left,
				InputSignalControl ("Source", Source),
				InputSignalControl ("Warp", Warp),
				Container.LabelAndControl ("Scale: ",
					new NumericEdit (Scale, false, 0.001f,
						React.By ((float s) => Scale = s).And (changed)), true));
		}

		protected override void Load (XElement xelem)
		{
			Scale = xelem.AttrFloat (nameof (Scale));
		}

		protected override void Save (XElement xelem)
		{
			xelem.SetAttributeValue (nameof (Scale), Scale);
		}

		public override IEnumerable<AnySignalEditor> Inputs
		{
			get { return EnumerableExt.Enumerate (Source, Warp); }
		}

		public override Signal<V, float> Signal
		{
			get { return Source.Signal.Warp (Warp.Signal.Cache ().Scale (Scale), Dv); }
		}

		public Value<float> Args
		{ 
			get { return KernelArg.Value (Scale); }
		}
	}
}

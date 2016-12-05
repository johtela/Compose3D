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


	internal class NormalMapEditor : SignalEditor<Vec2, Vec3>
	{
		public SignalEditor<Vec2, float> Source;
		public float Strength;
		public Vec2 Dv;

		protected override Control CreateControl ()
		{
			var changed = Changed.Adapt<float, AnySignalEditor> (this);
			return FoldableContainer.WithLabel ("Normal Map", true, HAlign.Left,
				InputSignalControl ("Source", Source),
				Container.LabelAndControl ("Strength: ",
					new NumericEdit (Strength, false, 1f,
						React.By ((float s) => Strength = s).And (changed)), true));
		}

		protected override void Load (XElement xelem)
		{
			Strength = xelem.AttrFloat (nameof (Strength));
		}

		protected override void Save (XElement xelem)
		{
			xelem.SetAttributeValue (nameof (Strength), Strength);
		}

		protected override string ToCode ()
		{
			return MethodSignature (Source.Name, "NormalMap", Name, Strength, Dv);
		}

		public override IEnumerable<AnySignalEditor> Inputs
		{
			get { return EnumerableExt.Enumerate (Source); }
		}

		public override Signal<Vec2, Vec3> Signal
		{
			get { return Source.Signal.Cache ().NormalMap (Strength, Dv); }
		}
	}
}

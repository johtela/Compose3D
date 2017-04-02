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
	using Textures;
	using System.Threading.Tasks;

	internal class NormalMapEditor : SignalEditor<Vec4>
	{
		public SignalEditor<float> Source;
		public float Strength;
		public Vec2 Dv;

		public NormalMapEditor (Texture texture)
			: base (ParSignalBuffer.NormalMap, texture) { }

		protected override Control CreateControl ()
		{
			var changed = Changed.Adapt<float, AnySignalEditor> (this);
			return FoldableContainer.WithLabel (Name, true, HAlign.Left,
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

		protected override void RenderToBuffer (Vec2i size)
		{
			ParSignalBuffer.NormalMap.ExecuteAsync (_queue,
				KernelArg.ReadBuffer (Source.Buffer),
				KernelArg.Value (Strength),
				KernelArg.WriteBuffer (Buffer),
				size.X, size.Y);
		}

		public override IEnumerable<AnySignalEditor> Inputs
		{
			get { return EnumerableExt.Enumerate (Source); }
		}

		public override Signal<Vec2, Vec4> Signal
		{
			get { return Source.Signal.Cache ().NormalMap (Strength, Dv); }
		}
	}
}
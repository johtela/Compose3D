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

	internal class BlendEditor : SignalEditor<Vec4>
	{
		public float BlendFactor;
		public SignalEditor<Vec4> Source;
		public SignalEditor<Vec4> Other;

		public BlendEditor (Texture texture)
			: base (ParSignalBuffer.Blend, texture) { }

		public override Signal<Vec2, Vec4> Signal
		{
			get { return Source.Signal.Blend (Other.Signal, BlendFactor); }
		}

		protected override Task RenderToBuffer (Vec2i size)
		{
			return ParSignalBuffer.Blend.ExecuteAsync (_queue,
				KernelArg.ReadBuffer (Source.Buffer),
				KernelArg.ReadBuffer (Other.Buffer),
				KernelArg.Value (BlendFactor),
				KernelArg.WriteBuffer (Buffer),
				size.X, size.Y);
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

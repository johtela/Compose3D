namespace Compose3D.Imaging.SignalEditors
{
	using System.Collections.Generic;
	using System.Xml.Linq;
	using Extensions;
	using CLTypes;
	using Imaging;
	using Visuals;
	using Maths;
	using Textures;
	using UI;
	using System.Threading.Tasks;

	internal class MaskEditor : SignalEditor<Vec4>
	{
		public SignalEditor<Vec4> Source;
		public SignalEditor<Vec4> Other;
		public SignalEditor<float> Mask;

		public MaskEditor (Texture texture)
			: base (ParSignalBuffer.Mask, texture) { }

		public override Signal<Vec2, Vec4> Signal
		{
			get { return Source.Signal.Mask (Other.Signal, Mask.Signal); }
		}

		public override IEnumerable<AnySignalEditor> Inputs
		{
			get { return EnumerableExt.Enumerate<AnySignalEditor> (Source, Other, Mask); }
		}

		protected override Control CreateControl ()
		{
			return FoldableContainer.WithLabel ("Blend", true, HAlign.Left,
				InputSignalControl ("Source", Source),
				InputSignalControl ("Other", Other),
				InputSignalControl ("Mask", Mask));
		}

		protected override Task RenderToBuffer (Vec2i size)
		{
			return ParSignalBuffer.Mask.ExecuteAsync (_queue,
				KernelArg.ReadBuffer (Source.Buffer),
				KernelArg.ReadBuffer (Other.Buffer),
				KernelArg.ReadBuffer (Mask.Buffer),
				KernelArg.WriteBuffer (Buffer),
				size.X, size.Y);
		}

		protected override void Load (XElement xelem)
		{
		}

		protected override void Save (XElement xelem)
		{
		}
	}
}

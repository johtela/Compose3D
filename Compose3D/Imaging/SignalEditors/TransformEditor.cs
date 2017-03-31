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

	internal class TransformEditor : SignalEditor<float>
	{
		public float Scale;
		public float Offset;
		public SignalEditor<float> Source;

		public TransformEditor (Texture texture)
			: base (ParSignalBuffer.Transform, texture) { }

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

		protected override void RenderToBuffer (Vec2i size)
		{
			ParSignalBuffer.Transform.ExecuteAsync (_queue,
				KernelArg.ReadBuffer (Source.Buffer),
				new TransformArgs (Scale, Offset),
				KernelArg.WriteBuffer (Buffer),
				size.X, size.Y);
		}

		public override Signal<Vec2, float> Signal
        {
            get { return Source.Signal.Scale (Scale).Offset (Offset).Clamp (0, 1f); }
        }

        public override IEnumerable<AnySignalEditor> Inputs
        {
            get { return EnumerableExt.Enumerate (Source); }
        }
    }
}

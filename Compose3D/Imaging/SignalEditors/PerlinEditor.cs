namespace Compose3D.Imaging.SignalEditors
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Xml.Linq;
	using Visuals;
	using CLTypes;
	using Reactive;
	using Maths;
	using UI;
	using Textures;
	using System.Threading.Tasks;

	internal class PerlinEditor : SignalEditor<float>
	{
		public int Seed;
		public Vec2 Scale;
		public bool Periodic;

		public PerlinEditor (Texture texture)
			: base (ParSignalBuffer.PerlinNoise, texture) { }

		protected override Control CreateControl ()
		{
			var changed = Changed.Adapt<int, AnySignalEditor> (this);
			var changedf = Changed.Adapt<float, AnySignalEditor> (this);
			return FoldableContainer.WithLabel ("Perlin Noise", true, HAlign.Left,
				Container.LabelAndControl ("Seed: ",
					new NumericEdit (Seed, true, 1f, React.By ((float s) => Seed = (int)s)
						.And (changedf)), true),
				Container.LabelAndControl ("Scale X: ",
					new NumericEdit (Scale.X, false, 1f, React.By ((float s) => Scale = new Vec2 (s, Scale.Y))
						.And (changedf)), true),
				Container.LabelAndControl ("Scale Y: ",
					new NumericEdit (Scale.Y, false, 1f, React.By ((float s) => Scale = new Vec2 (Scale.X, s))
						.And (changedf)), true),
				Container.LabelAndControl ("Periodic: ",
					new Picker (Periodic ? 1 : 0,
						React.By ((int i) => Periodic = i != 0).And (changed),
						"No", "Yes"), true));
		}

		protected override void Load (XElement xelem)
		{
			Seed = xelem.AttrInt (nameof (Seed));
			Scale = xelem.AttrVec<Vec2> (nameof (Scale));
			Periodic = xelem.AttrBool (nameof (Periodic));
		}

		protected override void Save (XElement xelem)
		{
			xelem.SetAttributeValue (nameof (Seed), Seed);
			xelem.SetAttributeValue (nameof (Scale), Scale);
			xelem.SetAttributeValue (nameof (Periodic), Periodic);
		}

		protected override Task RenderToBuffer (Vec2i size)
		{
			return ParSignalBuffer.PerlinNoise.ExecuteAsync (_queue,
				new PerlinArgs (Scale, Periodic),
				KernelArg.WriteBuffer (Buffer),
				size.X, size.Y);
		}

		public override IEnumerable<AnySignalEditor> Inputs
		{
			get { return Enumerable.Empty<AnySignalEditor> (); }
		}

		public override Signal<Vec2, float> Signal
		{
			get
			{
				var noiseGen = new PerlinNoise (Seed);
				var signal = Periodic ?
					new Signal<Vec3, float> (v => noiseGen.PeriodicNoise (v, new Vec3 (Scale, 256f))) :
					new Signal<Vec3, float> (noiseGen.Noise);
				return signal.MapInput ((Vec2 v) => new Vec3 (v * Scale, 0f)).NormalRangeTo0_1 ();
			}
		}
	}
}

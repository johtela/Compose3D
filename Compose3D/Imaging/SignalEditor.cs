namespace Compose3D.Imaging
{
	using System;
	using System.Linq;
	using System.Threading.Tasks;
	using Extensions;
	using Visuals;
	using Reactive;
	using Maths;
	using UI;

	public abstract class SignalEditor<T, U>
	{
		public abstract Signal<T, U> Signal { get; }
		public abstract Control Control { get; }
	}

	public static class SignalEditor
	{
		private class _Perlin : SignalEditor<Vec2, float>
		{
			public int Seed;
			public float Scale;

			public override Control Control
			{
				get
				{
					return FoldableContainer.WithLabel ("Perlin Noise", true, HAlign.Left,
						Container.LabelAndControl ("Seed: ",
							new NumericEdit (Seed, 1f, React.By ((float s) => Seed = (int)s)), true),
						Container.LabelAndControl ("Scale: ",
							new NumericEdit (Scale, 1f, React.By ((float s) => Scale = s)), true));
				}
			}

			public override Signal<Vec2, float> Signal
			{
				get
				{
					return new Signal<Vec3, float> (new PerlinNoise (Seed).Noise)
						.MapInput ((Vec2 v) => new Vec3 (v, 0f) * Scale);
				}
			}
		}

		public static SignalEditor<Vec2, float> Perlin (int seed, float scale)
		{
			return new _Perlin () { Seed = seed, Scale = scale };
		}
	}
}

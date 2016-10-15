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

		public Reaction<object> Changed { get; internal set; }
	}

	public static class SignalEditor
	{
		private class _Dummy<T, U> : SignalEditor<T, U>
		{
			public Signal<T, U> Source;

			public override Control Control
			{
				get { return null; }
			}

			public override Signal<T, U> Signal
			{
				get { return Source; }
			}
		}

		private class _Perlin : SignalEditor<Vec2, float>
		{
			public int Seed;
			public float Scale;

			public override Control Control
			{
				get
				{
					var changed = Changed.Adapt<float, object> (this);
					return FoldableContainer.WithLabel ("Perlin Noise", true, HAlign.Left,
						Container.LabelAndControl ("Seed: ",
							new NumericEdit (Seed, 1f, React.By ((float s) => Seed = (int)s).And (changed)), true),
						Container.LabelAndControl ("Scale: ",
							new NumericEdit (Scale, 1f, React.By ((float s) => Scale = s).And (changed)), true));
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

		private class _Warp<V> : SignalEditor<V, float>
			where V : struct, IVec<V, float>
		{
			public float Scale;
			public float Dx;
			public SignalEditor<V, float> Source;
			public SignalEditor<V, float> Warp;

			public override Control Control
			{
				get
				{
					var changed = Changed.Adapt<float, object> (this);
					return FoldableContainer.WithLabel ("Warp", true, HAlign.Left,
						Container.LabelAndControl ("Scale: ",
							new NumericEdit (Scale, Dx, React.By ((float s) => Scale = s).And (changed)), true));
				}
			}

			public override Signal<V, float> Signal
			{
				get
				{
					return Source.Signal.Warp (Warp.Signal.Scale (Scale), Dx);
				}
			}
		}

		private class _Colorize<T> : SignalEditor<T, Vec3>
		{
			public SignalEditor<T, float> Source;
			public ColorMap<Vec3> ColorMap;

			public override Control Control
			{
				get
				{
					var changed = Changed.Adapt<ColorMap<Vec3>, object> (this);
					return FoldableContainer.WithLabel ("Color Map", true, HAlign.Left,
						new ColorMapEdit (-1f, 1f, 20f, 200f, ColorMap, changed));
				}
			}

			public override Signal<T, Vec3> Signal
			{
				get { return Source.Signal.Colorize (ColorMap); }
			}
		}

		public static SignalEditor<T, U> ToSignalEditor<T, U> (this Signal<T, U> signal)
		{
			return new _Dummy<T, U> () { Source = signal };
		}

		public static SignalEditor<Vec2, float> Perlin (int seed, float scale, Reaction<object> changed)
		{
			return new _Perlin () { Seed = seed, Scale = scale, Changed = changed };
		}

		public static SignalEditor<V, float> Warp<V> (this SignalEditor<V, float> source, 
			SignalEditor<V, float> warp, float scale, float dx, Reaction<object> changed)
			where V : struct, IVec<V, float>
		{
			return new _Warp<V> () { Source = source, Warp = warp, Scale = scale, Dx = dx, Changed = changed };
		}

		public static SignalEditor<T, Vec3> Colorize<T> (this SignalEditor<T, float> source, 
			ColorMap<Vec3> colorMap, Reaction<object> changed)
		{
			return new _Colorize<T> () { Source = source, ColorMap = colorMap, Changed = changed };
		}
	}
}

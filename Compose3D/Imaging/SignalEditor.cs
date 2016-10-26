namespace Compose3D.Imaging
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Drawing;
	using Extensions;
	using Visuals;
	using Reactive;
	using Maths;
	using UI;

	public abstract class AnySignalEditor
	{
		private Connected _control;
		internal int _level;

		public Connected Control
		{
			get
			{
				if (_control == null)
					_control = new Connected (CreateControl (), HAlign.Right, VAlign.Center);
				return _control;
			}
		}

		protected Control InputSignalControl (string name, AnySignalEditor input)
		{
			return new Connector (Container.Frame (Label.Static (name)), input.Control,
				VisualDirection.Horizontal, HAlign.Left, VAlign.Center, ConnectorKind.Curved, 
				new VisualStyle (pen: new Pen (Color.OrangeRed, 3f)));
		}

		protected abstract Control CreateControl ();
		public abstract IEnumerable<AnySignalEditor> Inputs { get; }
		public Reaction<AnySignalEditor> Changed { get; internal set; }
	}

	public abstract class SignalEditor<T, U> : AnySignalEditor
	{
		public abstract Signal<T, U> Signal { get; }
	}

	public static class SignalEditor
	{
		private class _Dummy<T, U> : SignalEditor<T, U>
		{
			public Signal<T, U> Source;
			public string Name { get; internal set; }

			protected override Control CreateControl ()
			{
				return new Connected (
					Container.Frame (Label.Static (Name)),
					HAlign.Right, VAlign.Center);
			}

		 	public override IEnumerable<AnySignalEditor> Inputs
			{
				get { return Enumerable.Empty<AnySignalEditor> (); }
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

			protected override Control CreateControl ()
			{
				var changed = Changed.Adapt<float, AnySignalEditor> (this);
				return FoldableContainer.WithLabel ("Perlin Noise", true, HAlign.Left,
					Container.LabelAndControl ("Seed: ",
						new NumericEdit (Seed, true, 1f, React.By ((float s) => Seed = (int)s).And (changed)), true),
					Container.LabelAndControl ("Scale: ",
						new NumericEdit (Scale, false, 1f, React.By ((float s) => Scale = s).And (changed)), true));
			}

			public override IEnumerable<AnySignalEditor> Inputs
			{
				get { return Enumerable.Empty<AnySignalEditor> (); }
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

			public override IEnumerable<AnySignalEditor> Inputs
			{
				get { return EnumerableExt.Enumerate (Source, Warp); }
			}

			public override Signal<V, float> Signal
			{
				get { return Source.Signal.Warp (Warp.Signal.Scale (Scale), Dv); }
			}
		}

		private class _Colorize<T> : SignalEditor<T, Vec3>
		{
			public SignalEditor<T, float> Source;
			public ColorMap<Vec3> ColorMap;

			protected override Control CreateControl ()
			{
				var changed = Changed.Adapt<ColorMap<Vec3>, AnySignalEditor> (this);
				return FoldableContainer.WithLabel ("Color Map", true, HAlign.Left,
					InputSignalControl ("Source", Source),
					new ColorMapEdit (-1f, 1f, 20f, 200f, ColorMap, changed));
			}

			public override IEnumerable<AnySignalEditor> Inputs
			{
				get { return EnumerableExt.Enumerate (Source); }
			}

			public override Signal<T, Vec3> Signal
			{
				get { return Source.Signal.Colorize (ColorMap); }
			}
		}

		private class _SpectralControl<V> : SignalEditor<V, float>
			where V : struct, IVec<V, float>
		{
			public SignalEditor<V, float> Source;
			public int FirstBand;
			public int LastBand;
			public List<float> BandWeights;

			private Container _bandContainer;

			private Slider BandSlider (int band)
			{
				return new Slider (VisualDirection.Vertical, 16f, 100f, 0f, 1f, BandWeights [band],
					React.By ((float x) => BandWeights [band] = x)
					.And (Changed.Adapt<float, AnySignalEditor> (this)));
			}

			private void ChangeFirstBand (float fb)
			{
				var value = (int)fb;
				if (value >= 0 && value <= LastBand)
				{
					if (value < FirstBand)
						for (int i = value; i < FirstBand; i++)
							_bandContainer.Controls.Insert (0, BandSlider (i));
					else
						for (int i = FirstBand; i < value; i++)
							_bandContainer.Controls.RemoveAt (0);
					FirstBand = value;	
				}
			}

			private void ChangeLastBand (float lb)
			{
				var value = (int)lb;
				if (value < 16 && value >= FirstBand)
				{
					if (value > LastBand)
						for (int i = LastBand + 1; i <= value; i++)
							_bandContainer.Controls.Add (BandSlider (i));
					else
						for (int i = value; i < LastBand; i++)
							_bandContainer.Controls.RemoveAt (_bandContainer.Controls.Count - 1);
					LastBand = value;	
				}
			}

			protected override Control CreateControl ()
			{
				var changed = Changed.Adapt<float, AnySignalEditor> (this);
				var fbEdit = Container.LabelAndControl ("First Band: ",
					new NumericEdit (FirstBand, true, 1f, React.By<float> (ChangeFirstBand).And (changed)), true);
				var lbEdit = Container.LabelAndControl ("Last Band: ",
					new NumericEdit (LastBand, true, 1f, React.By<float> (ChangeLastBand).And (changed)), true);
				var sliders = Enumerable.Range (FirstBand, LastBand - FirstBand + 1)
					.Select (BandSlider).ToArray ();
				_bandContainer = Container.Horizontal (true, false, null, sliders);
				return FoldableContainer.WithLabel ("Spectral Control", true, HAlign.Left,
					InputSignalControl ("Source", Source),
					fbEdit, lbEdit, _bandContainer);
			}

			public override IEnumerable<AnySignalEditor> Inputs
			{
				get { return EnumerableExt.Enumerate (Source); }
			}

			public override Signal<V, float> Signal
			{
				get 
				{ 
					return Source.Signal.SpectralControl (FirstBand, LastBand, 
						BandWeights.Skip (FirstBand).Take (LastBand - FirstBand + 1).ToArray ()); 
				}
			}
		}

		private class _NormalMap : SignalEditor<Vec2, Vec3>
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

			public override IEnumerable<AnySignalEditor> Inputs
			{
				get { return EnumerableExt.Enumerate (Source); }
			}

			public override Signal<Vec2, Vec3> Signal
			{
				get { return Source.Signal.NormalMap (Strength, Dv); }
			}
		}

		public static SignalEditor<T, U> ToSignalEditor<T, U> (this Signal<T, U> signal, string name)
		{
			return new _Dummy<T, U> () { Name = name, Source = signal };
		}

		public static SignalEditor<Vec2, float> Perlin (int seed, float scale, 
			Reaction<AnySignalEditor> changed)
		{
			return new _Perlin () { Seed = seed, Scale = scale, Changed = changed };
		}

		public static SignalEditor<V, float> Warp<V> (this SignalEditor<V, float> source, 
			SignalEditor<V, float> warp, float scale, V dv, Reaction<AnySignalEditor> changed)
			where V : struct, IVec<V, float>
		{
			return new _Warp<V> () { Source = source, Warp = warp, Scale = scale, Dv = dv, Changed = changed };
		}

		public static SignalEditor<T, Vec3> Colorize<T> (this SignalEditor<T, float> source, 
			ColorMap<Vec3> colorMap, Reaction<AnySignalEditor> changed)
		{
			return new _Colorize<T> () { Source = source, ColorMap = colorMap, Changed = changed };
		}

		public static SignalEditor<V, float> SpectralControl<V> (this SignalEditor<V, float> source,
			int firstBand, int lastBand, float[] bandWeights, Reaction<AnySignalEditor> changed)
			where V : struct, IVec<V, float>
		{
			var bw = new List<float> (16);
			bw.AddRange (0f.Repeat (16));
			for (int i = firstBand; i <= lastBand; i++)
				bw [i] = bandWeights [i - firstBand];
			return new _SpectralControl<V> () { 
				Source = source, FirstBand = firstBand, LastBand = lastBand, BandWeights = bw, 
				Changed = changed 
			};
		}

		public static SignalEditor<Vec2, Vec3> NormalMap (this SignalEditor<Vec2, float> source,
			float strength, Vec2 dv, Reaction<AnySignalEditor> changed)
		{
			return new _NormalMap () { Source = source, Strength = strength, Dv = dv, Changed = changed };
		}

		private static void CollectInputEditors (AnySignalEditor editor, int level, HashSet<AnySignalEditor> all)
		{
			if (!all.Contains (editor))
				all.Add (editor);
			foreach (var input in editor.Inputs)
			{
				if (input._level >= level)
					input._level = level - 1;
				CollectInputEditors (input, level - 1, all);
			}
		}

		public static Container EditorTree (AnySignalEditor rootEditor)
		{
			var all = new HashSet<AnySignalEditor> ();
			CollectInputEditors (rootEditor, 0, all);
			var levelContainers = new List<Container> ();
			foreach (var level in all.GroupBy (e => e._level).OrderBy (g => g.Key))
			{
				var container = Container.Vertical (false, false, React.Ignore<Control> (),
					level.Select (e => e.Control));
				levelContainers.Add (container);
			}
			return Container.Horizontal (true, false, levelContainers);
		}
	}
}

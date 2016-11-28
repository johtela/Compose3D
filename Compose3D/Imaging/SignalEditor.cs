namespace Compose3D.Imaging
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Drawing;
	using System.Text;
	using Extensions;
	using Visuals;
	using Reactive;
	using Maths;
	using Textures;
	using UI;
	using OpenTK.Input;
	using OpenTK.Graphics.OpenGL4;

	public abstract class AnySignalEditor
	{
		private Connected _control;
		internal int _level;
		internal uint[] _buffer;

		public string Name { get; internal set; }

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

		protected string MethodSignature (string instance, string method, params object[] args)
		{
			return string.Format ("{0}.{1} ({2})", instance, method, 
				args.Select (CodeGen.ToCode).SeparateWith (", "));
		}

		internal string InitializationCode ()
		{
			return string.Format ("var {0} = {1};\n", Name, ToCode ());
		}

		protected abstract Control CreateControl ();
		protected abstract string ToCode ();

		public abstract IEnumerable<AnySignalEditor> Inputs { get; }
		public Reaction<AnySignalEditor> Changed { get; internal set; }
	}

	public abstract class SignalEditor<T, U> : AnySignalEditor
	{
		public abstract Signal<T, U> Signal { get; }
	}

	public enum ControlPointKind { Random, Halton23 }

	public enum DistanceFunctionKind { Euclidean, Squared, Manhattan }

	public static class SignalEditor
	{
		private class _Dummy<T, U> : SignalEditor<T, U>
		{
			public Signal<T, U> Source;

			protected override Control CreateControl ()
			{
				return new Connected (
					Container.Frame (Label.Static (Name)),
					HAlign.Right, VAlign.Center);
			}

			protected override string ToCode ()
			{
				return Name;
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
			public Vec2 Scale;
			public bool Periodic;

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

			protected override string ToCode ()
			{
				return MethodSignature ("SignalEditor", "Perlin", Name, Seed, Scale, Periodic); 
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
					return signal.MapInput ((Vec2 v) => new Vec3 (v * Scale, 0f));
				}
			}
		}

		private class _Worley : SignalEditor<Vec2, float>
		{
			public WorleyNoiseKind NoiseKind;
			public ControlPointKind ControlPoints;
			public DistanceFunctionKind DistanceFunction;
			public int ControlPointCount;
			public int Seed;
			public float Jitter;
			public bool Periodic;

			private Func<int, IEnumerable<Vec2>>[] _cpGenerators = {
				Imaging.Signal.RandomControlPoints<Vec2>,
				_ => Imaging.Signal.HaltonControlPoints ()
			};

			private Func<Vec2, Vec2, float>[] _distFunctions = {
				Vec.DistanceTo<Vec2>,
				Vec.SquaredDistanceTo<Vec2>,
				Vec.ManhattanDistanceTo<Vec2>
			};

			protected override Control CreateControl ()
			{
				var changed = Changed.Adapt<int, AnySignalEditor> (this);
				var changedf = Changed.Adapt<float, AnySignalEditor> (this);
				return FoldableContainer.WithLabel ("Worley Noise", true, HAlign.Left,
					Container.LabelAndControl ("Type: ",
						new Picker ((int)NoiseKind,
							React.By ((int i) => NoiseKind = (WorleyNoiseKind)i).And (changed),
							Enum.GetNames (typeof (WorleyNoiseKind))), true),
					Container.LabelAndControl ("Seed: ",
						new NumericEdit (Seed, true, 1,
							React.By ((float i) => Seed = (int)i).And (changedf)), true),
					Container.LabelAndControl ("CP Type: ",
						new Picker ((int)ControlPoints,
							React.By ((int i) => ControlPoints = (ControlPointKind)i).And (changed),
							Enum.GetNames (typeof (ControlPointKind))), true),
					Container.LabelAndControl ("CP Count: ",
						new NumericEdit (ControlPointCount, true, 1,
							React.By ((float i) => ControlPointCount = (int)i).And (changedf).Where (i => i > 2)), true),
					Container.LabelAndControl ("Distance: ",
						new Picker ((int)DistanceFunction,
							React.By ((int i) => DistanceFunction = (DistanceFunctionKind)i).And (changed),
							Enum.GetNames (typeof (DistanceFunctionKind))), true),
					Container.LabelAndControl ("Jitter: ",
						new NumericEdit (Jitter, false, 0.1f,
							React.By ((float x) => Jitter = x).And (changedf)), true),
					Container.LabelAndControl ("Periodic: ",
						new Picker (Periodic ? 1 : 0,
							React.By ((int i) => Periodic = i != 0).And (changed),
							"No", "Yes"), true));
			}

			protected override string ToCode ()
			{
				return MethodSignature ("SignalEditor", "Worley", Name, NoiseKind, ControlPoints, 
					ControlPointCount, Seed, DistanceFunction, Jitter, Periodic);
			}

			public override IEnumerable<AnySignalEditor> Inputs
			{
				get { return Enumerable.Empty<AnySignalEditor> (); }
			}

			public override Signal<Vec2, float> Signal
			{
				get
				{
					var cpoints = _cpGenerators[(int)ControlPoints] (Seed)
						.Take (ControlPointCount)
						.Jitter (Jitter);
					if (Periodic)
						cpoints = cpoints.ReplicateOnTorus ();
					var dfunc = _distFunctions[(int)DistanceFunction];
					return Imaging.Signal.WorleyNoise (NoiseKind, dfunc, cpoints);
				}
			}
		}

		private class _Transform<V> : SignalEditor<V, float>
			where V : struct, IVec<V, float>
		{
			public float Scale;
			public float Offset;
			public SignalEditor<V, float> Source;

			public override Signal<V, float> Signal
			{
				get { return Source.Signal.Scale (Scale).Offset (Offset).Clamp (-1f, 1f); }
			}

			public override IEnumerable<AnySignalEditor> Inputs
			{
				get { return EnumerableExt.Enumerate (Source); }
			}

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

			protected override string ToCode ()
			{
				return MethodSignature (Source.Name, "Transform", Name, Scale, Offset);
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

			protected override string ToCode ()
			{
				return MethodSignature (Source.Name, "Warp", Name, Warp, Scale, Dv);
			}

			public override IEnumerable<AnySignalEditor> Inputs
			{
				get { return EnumerableExt.Enumerate (Source, Warp); }
			}

			public override Signal<V, float> Signal
			{
				get { return Source.Signal.Warp (Warp.Signal.Cache ().Scale (Scale), Dv); }
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

			protected override string ToCode ()
			{
				return MethodSignature (Source.Name, "Colorize", Name, ColorMap);
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

			private Tuple<Control, Reaction<Control>> BandSlider (int band)
			{
				return new Tuple<Control, Reaction<Control>> (
					new Slider (VisualDirection.Vertical, 16f, 100f, 0f, 1f, BandWeights [band],
						React.By ((float x) => BandWeights [band] = x)
						.And (Changed.Adapt<float, AnySignalEditor> (this))),
					null);
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
				_bandContainer = Container.Horizontal (true, false, sliders);
				return FoldableContainer.WithLabel ("Spectral Control", true, HAlign.Left,
					InputSignalControl ("Source", Source),
					fbEdit, lbEdit, _bandContainer);
			}

			private IEnumerable<float> ActiveBandWeights ()
			{
				return BandWeights.Skip (FirstBand).Take (LastBand - FirstBand + 1);
			}

			protected override string ToCode ()
			{
				return MethodSignature (Source.Name, "SpectralControl", Name, FirstBand, LastBand, 
					ActiveBandWeights ());
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
						ActiveBandWeights ().ToArray ()); 
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

		public static SignalEditor<T, U> ToSignalEditor<T, U> (this Signal<T, U> signal, string name)
		{
			return new _Dummy<T, U> () { Name = name, Source = signal };
		}

		public static SignalEditor<Vec2, float> Perlin (string name, int seed, Vec2 scale, bool periodic)
		{
			return new _Perlin () { Name = name, Seed = seed, Scale = scale, Periodic = periodic };
		}

		public static SignalEditor<Vec2, float> Worley (string name, WorleyNoiseKind kind, 
			ControlPointKind controlPoints,	int controlPointCount, int seed, 
			DistanceFunctionKind distanceFunction, float jitter, bool periodic)
		{
			return new _Worley () { Name = name, NoiseKind = kind, ControlPoints = controlPoints,
				ControlPointCount = controlPointCount, Seed = seed, DistanceFunction = distanceFunction,
				Jitter = jitter, Periodic = periodic };
		}

		public static SignalEditor<V, float> Warp<V> (this SignalEditor<V, float> source, 
			string name, SignalEditor<V, float> warp, float scale, V dv)
			where V : struct, IVec<V, float>
		{
			return new _Warp<V> () { Name = name, Source = source, Warp = warp, Scale = scale, Dv = dv };
		}

		public static SignalEditor<V, float> Transform<V> (this SignalEditor<V, float> source,
			string name, float scale, float offset)
			where V : struct, IVec<V, float>
		{
			return new _Transform<V> () { Name = name, Source = source, Scale = scale, Offset = offset };
		}

		public static SignalEditor<T, Vec3> Colorize<T> (this SignalEditor<T, float> source, 
			string name, ColorMap<Vec3> colorMap)
		{
			return new _Colorize<T> () { Name = name, Source = source, ColorMap = colorMap };
		}

		public static SignalEditor<V, float> SpectralControl<V> (this SignalEditor<V, float> source,
			string name, int firstBand, int lastBand, params float[] bandWeights)
			where V : struct, IVec<V, float>
		{
			var bw = new List<float> (16);
			bw.AddRange (0f.Repeat (16));
			for (int i = firstBand; i <= lastBand; i++)
				bw [i] = bandWeights [i - firstBand];
			return new _SpectralControl<V> () { 
				Name = name, Source = source, FirstBand = firstBand, LastBand = lastBand, BandWeights = bw
			};
		}

		public static SignalEditor<Vec2, Vec3> NormalMap (this SignalEditor<Vec2, float> source,
			string name, float strength, Vec2 dv)
		{
			return new _NormalMap () { Name = name, Source = source, Strength = strength, Dv = dv };
		}

		private static void CollectInputEditors (AnySignalEditor editor, int level, 
			Reaction<AnySignalEditor> changed, HashSet<AnySignalEditor> all)
		{
			if (!all.Contains (editor))
			{
				if (changed != null)
					editor.Changed = changed;
				all.Add (editor);
			}
			foreach (var input in editor.Inputs)
			{
				if (input._level >= level)
					input._level = level - 1;
				CollectInputEditors (input, level - 1, changed, all);
			}
		}

		private static Signal<Vec2, uint> MapSignal (AnySignalEditor editor)
		{
			return
				editor is SignalEditor<Vec2, Vec3> ? 
					((SignalEditor<Vec2, Vec3>)editor).Signal
					.Vec3ToUintColor () :
				editor is SignalEditor<Vec2, float> ? 
					((SignalEditor<Vec2, float>)editor).Signal
					.Scale (0.5f)
					.Offset (0.5f)
					.FloatToUintGrayscale () :
				null;
		}

		public static Control EditorTree (Texture outputTexture, Vec2i outputSize, 
			DelayedReactionUpdater updater,	params AnySignalEditor[] rootEditors)
		{
			var all = new HashSet<AnySignalEditor> ();
			var changed = React.By ((AnySignalEditor editor) =>
			{
				editor._buffer = null;
				foreach (var edit in all.Where (e => e._level > editor._level))
					edit._buffer = null;
				editor._buffer = MapSignal (editor)
					.MapInput (Signal.BitmapCoordToUnitRange (outputSize, 1f))
					.SampleToBuffer (outputSize);
				outputTexture.LoadArray (editor._buffer, outputTexture.Target, 0, outputSize.X, outputSize.Y, 
					PixelFormat.Rgba, PixelInternalFormat.Rgb, PixelType.UnsignedInt8888);
			})
			.Delay (updater, 0.5);

			for (int i = 0; i < rootEditors.Length; i++)
				CollectInputEditors (rootEditors[i], 0, changed, all);
			var levelContainers = new List<Container> ();
			foreach (var level in all.GroupBy (e => e._level).OrderBy (g => g.Key))
			{
				var container = Container.Vertical (false, false, 
					level.Select (e => new Tuple<Control, Reaction<Control>> (
						e.Control, 
						React.By<Control> (_ => 
						{
							if (e._buffer == null)
								e._buffer = MapSignal (e)
									.MapInput (Signal.BitmapCoordToUnitRange (outputSize, 1f))
									.SampleToBuffer (outputSize);
							outputTexture.LoadArray (e._buffer, outputTexture.Target, 0, outputSize.X, outputSize.Y, 
								PixelFormat.Rgba, PixelInternalFormat.Rgb, PixelType.UnsignedInt8888);
						})
						.Delay (updater, 0.5)
					)));
				levelContainers.Add (container);
			}
			changed (rootEditors[0]);
			return new KeyboardCommand (Container.Horizontal (true, false, levelContainers),
				"Code copied to clipboard.",
				React.By ((Key key) => System.Windows.Forms.Clipboard.SetText (ToCode (rootEditors))),
				Key.C, Key.LControl);
		}

		public static string ToCode (params AnySignalEditor[] rootEditors)
		{
			var all = new HashSet<AnySignalEditor> ();
			for (int i = 0; i < rootEditors.Length; i++)
				CollectInputEditors (rootEditors[i], 0, null, all);

			var result = new StringBuilder ();
			foreach (var level in all.GroupBy (e => e._level).OrderBy (g => g.Key))
				foreach (var editor in level)
					result.Append (editor.InitializationCode ());
			return result.ToString ();
		}
	}
}

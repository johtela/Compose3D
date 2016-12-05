namespace Compose3D.Imaging.SignalEditors
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Drawing;
	using System.Text;
	using System.Xml.Linq;
	using Extensions;
	using Imaging;
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
		internal Texture _texture;

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
				new VisualStyle (pen: new Pen (Color.AliceBlue, 2f)));
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

		private string XElementName ()
		{
			var result = GetType ().Name;
			var i = result.IndexOf ('`');
			if (i > 0)
				result = result.Substring (0, i);
			return result;
		}

		internal XElement SaveToXml ()
		{
			var result = new XElement (XElementName (),
				new XAttribute ("Name", Name));
			Save (result);
			return result;
		}

		internal void LoadFromXml (XElement xml)
		{
			Load (xml.Elements (XElementName ()).Single (xe => xe.Attribute ("Name").Value == Name));
		}

		protected abstract Control CreateControl ();
		protected abstract string ToCode ();

		protected virtual void Load (XElement xelem) { }
		protected virtual void Save (XElement xelem) { }

		public abstract IEnumerable<AnySignalEditor> Inputs { get; }
		public Reaction<AnySignalEditor> Changed { get; internal set; }
	}

	public abstract class SignalEditor<T, U> : AnySignalEditor
	{
		public abstract Signal<T, U> Signal { get; }
	}

	public static class SignalEditor
	{
		public static SignalEditor<T, U> ToSignalEditor<T, U> (this Signal<T, U> signal, string name)
		{
			return new DummyEditor<T, U> () { Name = name, Source = signal };
		}

		public static SignalEditor<Vec2, float> Perlin (string name, Vec2 scale, int seed = 0, 
			bool periodic = false)
		{
			return new PerlinEditor () { Name = name, Seed = seed, Scale = scale, Periodic = periodic };
		}

		public static SignalEditor<Vec2, float> Worley (string name, WorleyNoiseKind kind = WorleyNoiseKind.F1, 
			ControlPointKind controlPoints = ControlPointKind.Random, int controlPointCount = 10, int seed = 0, 
			DistanceFunctionKind distanceFunction = DistanceFunctionKind.Euclidean, float jitter = 0f, 
			bool periodic = false)
		{
			return new WorleyEditor () { Name = name, NoiseKind = kind, ControlPoints = controlPoints,
				ControlPointCount = controlPointCount, Seed = seed, DistanceFunction = distanceFunction,
				Jitter = jitter, Periodic = periodic };
		}

		public static SignalEditor<V, float> Warp<V> (this SignalEditor<V, float> source, 
			string name, SignalEditor<V, float> warp, float scale, V dv)
			where V : struct, IVec<V, float>
		{
			return new WarpEditor<V> () { Name = name, Source = source, Warp = warp, Scale = scale, Dv = dv };
		}

		public static SignalEditor<V, float> Transform<V> (this SignalEditor<V, float> source,
			string name, float scale = 1f, float offset = 0f)
			where V : struct, IVec<V, float>
		{
			return new TransformEditor<V> () { Name = name, Source = source, Scale = scale, Offset = offset };
		}

		public static SignalEditor<T, Vec3> Colorize<T> (this SignalEditor<T, float> source, 
			string name, ColorMap<Vec3> colorMap)
		{
			return new ColorizeEditor<T> () { Name = name, Source = source, ColorMap = colorMap };
		}

		public static SignalEditor<V, float> SpectralControl<V> (this SignalEditor<V, float> source,
			string name, int firstBand, int lastBand, params float[] bandWeights)
			where V : struct, IVec<V, float>
		{
			var bw = new List<float> (16);
			bw.AddRange (0f.Repeat (16));
			for (int i = firstBand; i <= lastBand; i++)
				bw [i] = bandWeights [i - firstBand];
			return new SpectralControlEditor<V> () { 
				Name = name, Source = source, FirstBand = firstBand, LastBand = lastBand, BandWeights = bw
			};
		}

		public static SignalEditor<Vec2, Vec3> NormalMap (this SignalEditor<Vec2, float> source,
			string name, float strength, Vec2 dv)
		{
			return new NormalMapEditor () { Name = name, Source = source, Strength = strength, Dv = dv };
		}

		public static AnySignalEditor ToTexture (this AnySignalEditor editor, Texture texture)
		{
			editor._texture = texture;
			return editor;
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

		public static Control EditorUI (Texture outputTexture, Vec2i outputSize, 
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
			return new CommandContainer (Container.Horizontal (true, false, levelContainers),
				new KeyboardCommand ("Code copied to clipboard.",
					React.By ((Key key) => System.Windows.Forms.Clipboard.SetText (ToCode (rootEditors))),
					Key.C, Key.LControl));
		}

		public static Control EditorUI (string filePath, Texture outputTexture, Vec2i outputSize,
			DelayedReactionUpdater updater, params AnySignalEditor[] rootEditors)
		{
			if (File.Exists (filePath))
				LoadFromFile (filePath, rootEditors);
			return new CommandContainer (EditorUI (outputTexture, outputSize, updater, rootEditors),
				new KeyboardCommand ("Saved to file: " + filePath,
					React.By ((Key key) => SaveToFile (filePath, rootEditors)),
					Key.S, Key.LControl));
		}


		private static IEnumerable<AnySignalEditor> EditorsByLevel (AnySignalEditor[] rootEditors)
		{
			var all = new HashSet<AnySignalEditor> ();
			for (int i = 0; i < rootEditors.Length; i++)
				CollectInputEditors (rootEditors[i], 0, null, all);

			return from level in all.GroupBy (e => e._level).OrderBy (g => g.Key)
				   from editor in level
				   select editor;
		}

		public static string ToCode (params AnySignalEditor[] rootEditors)
		{
			var result = new StringBuilder ();
			foreach (var editor in EditorsByLevel (rootEditors))
				result.Append (editor.InitializationCode ());
			return result.ToString ();
		}

		public static XElement SaveToXml (params AnySignalEditor[] rootEditors)
		{
			return new XElement ("SignalEditors",
				from editor in EditorsByLevel (rootEditors)
				select editor.SaveToXml ());
		}

		public static void LoadFromXml (XElement xml, params AnySignalEditor[] rootEditors)
		{
			foreach (var editor in EditorsByLevel (rootEditors))
				editor.LoadFromXml (xml);
		}

		public static void SaveToFile (string filePath, params AnySignalEditor[] rootEditors)
		{
			SaveToXml (rootEditors).Save (filePath);
		}

		public static void LoadFromFile (string filePath, params AnySignalEditor[] rootEditors)
		{
			LoadFromXml (XElement.Load (filePath), rootEditors);
		}
	}
}
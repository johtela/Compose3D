namespace Compose3D.Imaging.SignalEditors
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Xml.Linq;
	using OpenTK.Graphics.OpenGL4;
	using Extensions;
	using Imaging;
	using Reactive;
	using Maths;
	using Textures;
	using UI;
	using OpenTK.Input;

	public abstract class SignalEditor<T> : AnySignalEditor
		where T : struct
	{
		public SignalEditor ()
			: base (null) { }

		protected override void AllocateBuffer (int length)
		{
			if (Buffer == null || Buffer.Length != length)
				Buffer = new T[length];
		}

		protected override void RenderToBuffer (Vec2i size)
		{
			Signal.MapInput (Imaging.Signal.BitmapCoordToUnitRange (size, 1f))
				.SampleToBuffer (Buffer, size);
		}

		protected override void UpdateTexture (Vec2i size)
		{
			if (this is SignalEditor<float>)
				Texture.LoadArray (Buffer, Texture.Target, 0, size.X, size.Y,
					PixelFormat.Red, PixelInternalFormat.CompressedSignedRedRgtc1, PixelType.Float);
			else
				Texture.LoadArray (Buffer, Texture.Target, 0, size.X, size.Y,
					PixelFormat.Rgb, PixelInternalFormat.Rgb32f, PixelType.Float);
		}

		public T[] Buffer { get; private set; }

		public abstract Signal<Vec2, T> Signal { get; }
	}

	public static class SignalEditor
	{
		const float UpdateDelay = 0.2f;

		public static SignalEditor<T> ToSignalEditor<T> (this Signal<Vec2, T> signal, string name)
			where T : struct
		{
			return new DummyEditor<T> () { Name = name, Source = signal };
		}

		public static SignalEditor<float> Perlin (string name, Vec2 scale, int seed = 0, 
			bool periodic = false)
		{
			return new PerlinEditor () { Name = name, Seed = seed, Scale = scale, Periodic = periodic };
		}

		public static SignalEditor<float> Worley (string name, WorleyNoiseKind kind = WorleyNoiseKind.F1, 
			ControlPointKind controlPoints = ControlPointKind.Random, int controlPointCount = 10, int seed = 0, 
			DistanceKind distanceKind = DistanceKind.Euclidean, float jitter = 0f, 
			bool periodic = false)
		{
			return new WorleyEditor () { Name = name, NoiseKind = kind, ControlPoints = controlPoints,
				ControlPointCount = controlPointCount, Seed = seed, DistanceKind = distanceKind,
				Jitter = jitter, Periodic = periodic };
		}

		public static SignalEditor<float> Warp (this SignalEditor<float> source, 
			string name, SignalEditor<float> warp, float scale, Vec2 dv)
		{
			return new WarpEditor () { Name = name, Source = source, Warp = warp, Scale = scale, Dv = dv };
		}

		public static SignalEditor<float> Blend (this SignalEditor<float> source,
			string name, SignalEditor<float> other, float blendFactor)
		{
			return new BlendEditor () { Name = name, Source = source, Other = other, BlendFactor = blendFactor };
		}

		public static SignalEditor<float> Mask(this SignalEditor<float> source,
			string name, SignalEditor<float> other, SignalEditor<float> mask)
		{
			return new MaskEditor () { Name = name, Source = source, Other = other, Mask = mask };
		}

		public static SignalEditor<float> MaskWithSource<V> (this SignalEditor<float> source,
			string name, SignalEditor<float> other)
		{
			return new MaskEditor () { Name = name, Source = source, Other = other, Mask = source };
		}

		public static SignalEditor<float> Transform (this SignalEditor<float> source,
			string name, float scale = 1f, float offset = 0f)
		{
			return new TransformEditor () { Name = name, Source = source, Scale = scale, Offset = offset };
		}

		public static SignalEditor<Vec3> Colorize (this SignalEditor<float> source, 
			string name, ColorMap<Vec3> colorMap)
		{
			return new ColorizeEditor () { Name = name, Source = source, ColorMap = colorMap };
		}

		public static SignalEditor<float> SpectralControl (this SignalEditor<float> source,
			string name, int firstBand, int lastBand, params float[] bandWeights)
		{
			var bw = new List<float> (16);
			bw.AddRange (0f.Repeat (16));
			for (int i = firstBand; i <= lastBand; i++)
				bw [i] = bandWeights [i - firstBand];
			return new SpectralControlEditor () { 
				Name = name, Source = source, FirstBand = firstBand, LastBand = lastBand, BandWeights = bw
			};
		}

		public static SignalEditor<Vec3> NormalMap (this SignalEditor<float> source,
			string name, float strength, Vec2 dv)
		{
			return new NormalMapEditor () { Name = name, Source = source, Strength = strength, Dv = dv };
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

		public static Control EditorUI (Texture outputTexture, Vec2i outputSize, 
            DelayedReactionUpdater delayedUpdater, params AnySignalEditor[] rootEditors)
		{
			var all = new HashSet<AnySignalEditor> ();
			var changed = React.By ((AnySignalEditor editor) =>
			{
				foreach (var edit in all.Where (e => e._level > editor._level))
					edit._updated = false;
                editor._updated = false;
				editor.Render (outputSize);
			})
			.Delay (delayedUpdater, UpdateDelay);

			for (int i = 0; i < rootEditors.Length; i++)
				CollectInputEditors (rootEditors[i], 0, changed, all);
			var levelContainers = new List<Container> ();
			foreach (var level in all.GroupBy (e => e._level).OrderBy (g => g.Key))
			{
				var container = Container.Vertical (false, false, 
					level.Select (e => new Tuple<Control, Reaction<Control>> (
						e.Control, 
						React.By<Control> (_ => e.Render (outputSize))
						.Delay (delayedUpdater, UpdateDelay)
					)));
				levelContainers.Add (container);
			}
			changed (rootEditors[0]);
            return Container.Horizontal (true, false, levelContainers);
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
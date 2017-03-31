namespace Compose3D.Imaging.SignalEditors
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using OpenTK.Graphics.OpenGL4;
	using Extensions;
	using CLTypes;
	using Imaging;
	using Reactive;
	using Maths;
	using Textures;
	using UI;
	using OpenTK.Input;

	public abstract class SignalEditor<T> : AnySignalEditor
		where T : struct
	{
		public SignalEditor (CLKernel kernel, Texture texture)
			: base (kernel, texture) { }

		protected override void AllocateBuffer (int length)
		{
			if (Buffer == null || Buffer.Length != length)
				Buffer = new T[length];
		}

		protected override Task RenderToBuffer (Vec2i size)
		{
			return Task.Factory.StartNew (() =>
				Signal.MapInput (Imaging.Signal.BitmapCoordTo0_1 (size, 1f))
					.SampleToBuffer (Buffer, size));
		}

		protected override void UpdateTexture (Vec2i size)
		{
			if (this is SignalEditor<float>)
				Texture.LoadArray (Buffer, Texture.Target, 0, size.X, size.Y,
					PixelFormat.Red, PixelInternalFormat.CompressedSignedRedRgtc1, PixelType.Float);
			else
				Texture.LoadArray (Buffer, Texture.Target, 0, size.X, size.Y,
					PixelFormat.Rgba, PixelInternalFormat.Rgba32f, PixelType.Float);
		}

		public T[] Buffer { get; private set; }

		public abstract Signal<Vec2, T> Signal { get; }
	}

	public static class SignalEditor
	{
		const float UpdateDelay = 0.5f;

		public static SignalEditor<T> ToSignalEditor<T> (this Signal<Vec2, T> signal, string name,
			Texture texture = null)
			where T : struct
		{
			return new DummyEditor<T> (texture) { Name = name, Source = signal };
		}

		public static SignalEditor<float> Perlin (string name, Vec2 scale, int seed = 0, 
			bool periodic = false, Texture texture = null)
		{
			return new PerlinEditor (texture) { Name = name, Seed = seed, Scale = scale,
				Periodic = periodic };
		}

		public static SignalEditor<float> Worley (string name, WorleyNoiseKind kind = WorleyNoiseKind.F1, 
			ControlPointKind controlPoints = ControlPointKind.Random, int controlPointCount = 10, 
			int seed = 0, DistanceKind distanceKind = DistanceKind.Euclidean, float jitter = 0f, 
			bool periodic = false, Texture texture = null)
		{
			return new WorleyEditor (texture) { Name = name, NoiseKind = kind,
				ControlPoints = controlPoints, ControlPointCount = controlPointCount, Seed = seed,
				DistanceKind = distanceKind, Jitter = jitter, Periodic = periodic };
		}

		public static SignalEditor<float> Warp (this SignalEditor<float> source, 
			string name, SignalEditor<float> warp, float scale, Vec2 dv, Texture texture = null)
		{
			return new WarpEditor (texture) { Name = name, Source = source, Warp = warp,
				Scale = scale, Dv = dv };
		}

		public static SignalEditor<Vec4> Blend (this SignalEditor<Vec4> source,
			string name, SignalEditor<Vec4> other, float blendFactor, Texture texture = null)
		{
			return new BlendEditor (texture) { Name = name, Source = source, Other = other,
				BlendFactor = blendFactor };
		}

		public static SignalEditor<Vec4> Mask(this SignalEditor<Vec4> source,
			string name, SignalEditor<Vec4> other, SignalEditor<float> mask, 
			Texture texture = null)
		{
			return new MaskEditor (texture) { Name = name, Source = source, Other = other,
				Mask = mask };
		}

		public static SignalEditor<float> Transform (this SignalEditor<float> source,
			string name, float scale = 1f, float offset = 0f, Texture texture = null)
		{
			return new TransformEditor (texture) { Name = name, Source = source, Scale = scale,
				Offset = offset };
		}

		public static SignalEditor<Vec4> Colorize (this SignalEditor<float> source, 
			string name, ColorMap<Vec3> colorMap, Texture texture = null)
		{
			return new ColorizeEditor (texture) { Name = name, Source = source,
				ColorMap = colorMap };
		}

		public static SignalEditor<float> SpectralControl (this SignalEditor<float> source,
			string name, int firstBand, int lastBand, Texture texture = null, 
			params float[] bandWeights)
		{
			var bw = new List<float> (16);
			bw.AddRange (0f.Repeat (16));
			for (int i = firstBand; i <= lastBand; i++)
				bw [i] = bandWeights [i - firstBand];
			return new SpectralControlEditor (texture) { 
				Name = name, Source = source, FirstBand = firstBand, LastBand = lastBand,
				BandWeights = bw
			};
		}

		public static SignalEditor<Vec4> NormalMap (this SignalEditor<float> source,
			string name, float strength, Vec2 dv, Texture texture = null)
		{
			return new NormalMapEditor (texture) { Name = name, Source = source, Strength = strength,
				Dv = dv };
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

		public static Control EditorUI (Vec2i outputSize, Reaction<Texture> selected,
			DelayedReactionUpdater delayedUpdater, params AnySignalEditor[] rootEditors)
		{
			var all = new HashSet<AnySignalEditor> ();
			var changed = React.By ((AnySignalEditor editor) =>
			{
				editor.Render (outputSize);
				var updatedLevels = from edit in all
									where edit._level > editor._level
									group edit by edit._level into level
									orderby level.Key
									select level;
				foreach (var level in updatedLevels)
					foreach (var edit in level)
						edit.Render (outputSize);
			})
			.Delay (delayedUpdater, UpdateDelay);

			for (int i = 0; i < rootEditors.Length; i++)
				CollectInputEditors (rootEditors[i], 0, changed, all);
			var levelContainers = new List<Container> ();
			foreach (var level in all.GroupBy (e => e._level).OrderBy (g => g.Key))
			{
				foreach (var editor in level)
					editor.Render (outputSize);
				var container = Container.Vertical (false, false, 
					level.Select (e => new Tuple<Control, Reaction<Control>> (
						e.Control, selected.MapInput ((Control _) => e.Texture))));
				levelContainers.Add (container);
			}
            return Container.Horizontal (true, false, levelContainers);
		}

		public static Control EditorUI (string filePath, Vec2i outputSize, Reaction<Texture> updated, 
			DelayedReactionUpdater updater, params AnySignalEditor[] rootEditors)
		{
			if (File.Exists (filePath))
				LoadFromFile (filePath, rootEditors);
			return new CommandContainer (EditorUI (outputSize, updated, updater, rootEditors),
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
namespace Compose3D.Imaging
{
	using CLTypes;
	using Maths;
	using Reactive;
	using SignalEditors;
	using Textures;

	public class Ground : Material
    {
        public Ground (Vec2i size, string fileName, Texture texture, DelayedReactionUpdater updater)
            : base (size, fileName, texture, updater) { }

        protected override AnySignalEditor[] RootEditors ()
        {
			var worley = SignalEditor.Worley (Worley.Name,
					WorleyNoiseKind.F1, ControlPointKind.Random,
					10, 0, DistanceKind.Euclidean, 0f, true)
				.Render ((e, s) => RunKernel (e, s, Worley, e.Args));
			var transform = worley.Transform (Transform.Name, -30f, 0.5f)
				.Render ((e, s) => RunKernel (e, s, Transform, worley.Args, e.Args));
			var dv = new Vec2 (1f) / new Vec2 (Size.X, Size.Y);
            var perlin = SignalEditor.Perlin (Perlin.Name, new Vec2 (10f))
				.Render ((e, s) => RunKernel (e, s, Perlin, e.Args));
            var spectral = perlin.SpectralControl (Spectral.Name, 0, 2, 1f, 0.5f, 0.2f)
				.Render ((e, s) => RunKernel (e, s, Spectral, perlin.Args, e.Args));
            var warp = transform.Warp (Warp.Name, spectral, 0.001f, dv)
				.Render ((e, s) => RunKernel (e, s, Warp, worley.Args, transform.Args, perlin.Args, 
					spectral.Args, e.Args));
            var signal = warp.Colorize ("Signal", ColorMap<Vec3>.GrayScale ());
            var normal = warp.NormalMap ("Normal", 1f, dv);

            return new AnySignalEditor[] { normal, signal };
        }

		static readonly CLKernel<WorleyArgs, Buffer<uint>>	
			Worley = CLKernel.Create 
			(
				nameof (Worley),
				(WorleyArgs wor, Buffer<uint> result) =>
					from pos in ParSignal.PixelPosTo0_1 ().ToKernel ()
					let v = ParSignal.WorleyNoise (wor.ControlPoints, !wor.Count, !wor.DistanceKind, 
						!wor.NoiseKind, pos)
					let gs = ParSignal.GrayscaleToUint (ParSignal.NormalRangeTo0_1 (v))
					select new KernelResult { Assign.Buffer (result, ParSignal.PixelPosToIndex (), gs) }
			);

		static readonly CLKernel<WorleyArgs, TransformArgs, Buffer<uint>>
			Transform = CLKernel.Create
			(
				nameof (Transform),
				(WorleyArgs wor, TransformArgs tx, Buffer<uint> result) =>
					from pos in ParSignal.PixelPosTo0_1 ().ToKernel ()
					let v = ParSignal.Transform (!tx.Scale, !tx.Offset,
						ParSignal.WorleyNoise (wor.ControlPoints, !wor.Count, !wor.DistanceKind, 
						!wor.NoiseKind, pos))
					let gs = ParSignal.GrayscaleToUint (ParSignal.NormalRangeTo0_1 (v))
					select new KernelResult { Assign.Buffer (result, ParSignal.PixelPosToIndex (), gs) }
			);

		static readonly CLKernel<PerlinArgs, Buffer<uint>>
			Perlin = CLKernel.Create
			(
				nameof (Perlin),
				(PerlinArgs per, Buffer<uint> result) =>
					from pos in ParSignal.PixelPosTo0_1 ().ToKernel ()
					let v = ParSignal.PerlinNoise (!per.Scale, !per.Periodic, pos)
					let gs = ParSignal.GrayscaleToUint (ParSignal.NormalRangeTo0_1 (v))
					select new KernelResult { Assign.Buffer (result, ParSignal.PixelPosToIndex (), gs) }
			);

		static readonly CLKernel<PerlinArgs, SpectralControlArgs, Buffer<uint>>
			Spectral = CLKernel.Create
			(
				nameof (Spectral),
				(PerlinArgs per, SpectralControlArgs spec, Buffer<uint> result) =>
					from pos in ParSignal.PixelPosTo0_1 ().ToKernel ()
					let v = ParSignal.SpectralControl (
						p => ParSignal.PerlinNoise (!per.Scale, !per.Periodic, p),
						!spec.FirstBand, !spec.LastBand, spec.NormalizedWeights, pos)
					let gs = ParSignal.GrayscaleToUint (ParSignal.NormalRangeTo0_1 (v))
					select new KernelResult { Assign.Buffer (result, ParSignal.PixelPosToIndex (), gs) }
			);

		static readonly CLKernel<WorleyArgs, TransformArgs, PerlinArgs, SpectralControlArgs, Value<float>, Buffer<uint>>
			Warp = CLKernel.Create
			(
				nameof (Warp),
				(WorleyArgs wor, TransformArgs tx, PerlinArgs per, SpectralControlArgs spec, Value<float> scale, 
					Buffer<uint> result) =>
					from pos in ParSignal.PixelPosTo0_1 ().ToKernel ()
					let res = ParSignal.Warp (
						p1 =>
							ParSignal.Transform (!tx.Scale, !tx.Offset,
								ParSignal.WorleyNoise (wor.ControlPoints, !wor.Count, !wor.DistanceKind,
								!wor.NoiseKind, p1)),
						p2 => ParSignal.SpectralControl (
							p => ParSignal.PerlinNoise (!per.Scale, !per.Periodic, p) * !scale,
								!spec.FirstBand, !spec.LastBand, spec.NormalizedWeights, p2),
						pos)
					let gs = ParSignal.GrayscaleToUint (ParSignal.NormalRangeTo0_1 (res))
					select new KernelResult { Assign.Buffer (result, ParSignal.PixelPosToIndex (), gs) }
			);

		public static CLKernel<PerlinArgs, ColorizeArgs<Vec4>, SpectralControlArgs, Buffer<uint>>
			PerlinSignal = CLKernel.Create
			(
				nameof (PerlinSignal),
				(PerlinArgs perlin, ColorizeArgs<Vec4> colorMap, SpectralControlArgs spectral, Buffer<uint> result) =>
					from pos in ParSignal.PixelPosTo0_1 ().ToKernel ()
					let t = ParSignal.NormalMap (
						v1 => ParSignal.SpectralControl (
							v2 => ParSignal.PerlinNoise (!perlin.Scale, !perlin.Periodic, v2),
							!spectral.FirstBand, !spectral.LastBand, spectral.NormalizedWeights, v1),
						1f, pos)
					let col = ParSignal.Color4ToUint (
						ParSignal.Colorize (colorMap.Keys, colorMap.Colors, !colorMap.Count, t.Item1))
					let gs = ParSignal.GrayscaleToUint (ParSignal.NormalRangeTo0_1 (t.Item1))
					select new KernelResult
					{
						Assign.Buffer (result, ParSignal.PixelPosToIndex (), col)
					}
			);

		public static CLKernel<UniformWorleyArgs, ColorizeArgs<Vec4>, Buffer<uint>>
			UniformWorleySignal = CLKernel.Create
			(
				nameof (UniformWorleySignal),
				(UniformWorleyArgs worley, ColorizeArgs<Vec4> colorMap, Buffer<uint> result) =>
					from pos in ParSignal.PixelPosTo0_1 ().ToKernel ()
					let v = ParSignal.UniformWorleyNoise (!worley.Scale, !worley.Jitter, !worley.DistanceKind,
						!worley.NoiseKind, pos)
					let col = ParSignal.Color4ToUint (
						ParSignal.Colorize (colorMap.Keys, colorMap.Colors, !colorMap.Count, v))
					let gs = ParSignal.GrayscaleToUint (ParSignal.NormalRangeTo0_1 (v))
					select new KernelResult
					{
						Assign.Buffer (result, ParSignal.PixelPosToIndex (), gs)
					}
			);

		public static CLKernel<WorleyArgs, ColorizeArgs<Vec4>, Buffer<uint>>
			WorleySignal = CLKernel.Create
			(
				nameof (WorleySignal),
				(WorleyArgs worley, ColorizeArgs<Vec4> colorMap, Buffer<uint> result) =>
					from pos in ParSignal.PixelPosTo0_1 ().ToKernel ()
					let v = ParSignal.WorleyNoise (worley.ControlPoints, !worley.Count, !worley.DistanceKind,
						!worley.NoiseKind, pos) * 2f
					let col = ParSignal.Color4ToUint (
						ParSignal.Colorize (colorMap.Keys, colorMap.Colors, !colorMap.Count, v))
					let gs = ParSignal.GrayscaleToUint (ParSignal.NormalRangeTo0_1 (v))
					select new KernelResult
					{
						Assign.Buffer (result, ParSignal.PixelPosToIndex (), gs)
					}
			);
	}
}

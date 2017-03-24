namespace Compose3D.Imaging
{
	using System;
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
				.Render ((e, s) => Render (e, s, Worley, e.Args));
			var transform = worley.Transform ("transform", -30f, 0.5f);
            var dv = new Vec2 (1f) / new Vec2 (Size.X, Size.Y);
            var perlin = SignalEditor.Perlin ("perlin", new Vec2 (10f));
            var spectral = perlin.SpectralControl ("spectral", 0, 2, 1f, 0.5f, 0.2f);
            var warp = transform.Warp ("warp", spectral, 0.001f, dv);
            var signal = warp.Colorize ("signal", ColorMap<Vec3>.GrayScale ());
            var normal = warp.NormalMap ("normal", 1f, dv);

            return new AnySignalEditor[] { normal, signal };
        }

		protected override CLKernel[] Kernels ()
		{
			return new CLKernel[] { Worley };
		}

		static readonly CLKernel<WorleyArgs, Buffer<uint>>	
			Worley = CLKernel.Create 
			(
				nameof (Worley),
				(WorleyArgs args, Buffer<uint> result) =>
					from gs in ParSignal.SignalToGrayscale (
						pos => ParSignal.WorleyNoise (
							args.ControlPoints, !args.Count, !args.DistanceKind, !args.NoiseKind, pos))
						.ToKernel ()
					select new KernelResult
					{
						Assign.Buffer (result, ParSignal.PixelPosToIndex (), gs)
					}
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

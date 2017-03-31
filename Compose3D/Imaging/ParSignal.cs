namespace Compose3D.Imaging
{
	using System;
	using System.Linq;
	using Compiler;
	using CLTypes;
	using Maths;

	public static class ParSignal
	{
		public static readonly Func<float, uint> 
			GrayscaleToUint = CLKernel.Function 
			(
				() => GrayscaleToUint,
				val => Kernel.Evaluate
				(
					from c in ((uint)(val.Clamp (0f, 1f) * 255f)).ToKernel ()
					select c << 24 | c << 16 | c << 8 | 255
				)
			);

		public static readonly Func<Vec3, uint> 
			Color3ToUint = CLKernel.Function 
			(
				() => Color3ToUint,
				vec => Kernel.Evaluate
				(
					from h in 255f.ToKernel ()
					select (uint)(vec.X.Clamp (0f, 1f) * h) << 24 |
						(uint)(vec.Y.Clamp (0f, 1f) * h) << 16 |
						(uint)(vec.Z.Clamp (0f, 1f) * h) << 8 | 255
				)
			);

		public static readonly Func<Vec4, uint>
			Color4ToUint = CLKernel.Function
			(
				() => Color4ToUint,
				vec => Kernel.Evaluate
				(
					from h in 255f.ToKernel ()
					select (uint)(vec.X.Clamp (0f, 1f) * h) << 24 |
						(uint)(vec.Y.Clamp (0f, 1f) * h) << 16 |
						(uint)(vec.Z.Clamp (0f, 1f) * h) << 8 |
						(uint)(vec.W.Clamp (0f, 1f) * h)
				)
			);
		public static readonly Func<Vec2, int, Vec2, float> 
			PerlinNoise = CLKernel.Function 
			(
				() => PerlinNoise,
				(scale, periodic, pos) => Kernel.Evaluate
				(
					from scaled in (pos * scale).ToKernel ()
					let val = periodic == 0 ?
						ParPerlin.Noise (new Vec3 (scaled, 0f)) :
						ParPerlin.PeriodicNoise (new Vec3 (scaled, 0f), new Vec3 (scale, 256f))
					select val * 0.5f + 0.5f
				)
			);

        public static readonly Func<Buffer<Vec2>, int, int, int, Vec2, float>
            WorleyNoise = CLKernel.Function
            (
                () => WorleyNoise,
                (controlPoints, count, distKind, noiseKind, pos) => Kernel.Evaluate
                (
                    from res in ParWorley.Noise2DCP (controlPoints, count, distKind, pos).ToKernel ()
                    let val =
                        noiseKind == (int)WorleyNoiseKind.F1 ? res.X :
                        noiseKind == (int)WorleyNoiseKind.F2 ? res.Y :
                        noiseKind == (int)WorleyNoiseKind.F3 ? res.Z :
                        res.Y - res.X
					select val.Clamp (0f, 1f)
                )
            );

        public static readonly Func<Vec2, float, int, int, Vec2, float>
			UniformWorleyNoise = CLKernel.Function
			(
				() => UniformWorleyNoise,
				(scale, jitter, distKind, noiseKind, pos) => Kernel.Evaluate
				(
					from scaled in (pos * scale).ToKernel ()
					let res = ParWorley.Noise2D (scaled, jitter, distKind)
					select
						noiseKind == (int)WorleyNoiseKind.F1 ? res.X :
						noiseKind == (int)WorleyNoiseKind.F2 ? res.Y :
						res.Y - res.X
				)
			);

		public static readonly Func<Vec2> 
			Dv = CLKernel.Function 
			(
				() => Dv,
				() => new Vec2 (1f / Kernel.GetGlobalSize (0), 1f / Kernel.GetGlobalSize (1))
			);

		public static readonly Func<float, float> 
			NormalRangeTo0_1 = CLKernel.Function 
			(
				() => NormalRangeTo0_1,
				val => val * 0.5f + 0.5f
			);

		public static readonly Func<float, float, float, float>
			Transform = CLKernel.Function
			(
				() => Transform, 
				(scale, offset, val) => ((val * scale) + offset).Clamp (-1f, 1f)
			);

		public static readonly Macro<Macro<float, float>, float, float, float> 
			Dfdx = CLKernel.Macro 
			(
				() => Dfdx,
				(signal, dx, x) => (signal (x + dx) - signal (x)) / dx
			);

		public static readonly Func<Vec2, int, float, Vec2> 
			Vec2With = CLKernel.Function 
			(
				() => Vec2With,
				(vec, index, value) => index == 0 ? 
					new Vec2 (value, vec.Y) :
					new Vec2 (vec.X, value)
			);

		public static readonly Macro<Macro<Vec2, float>, Vec2, Vec2, CLTuple<float, Vec2>> 
			Dfdv2 = CLKernel.Macro 
			(
				() => Dfdv2,
				(signal, dv, vec) => Kernel.Evaluate
				(
					from value in signal (vec).ToKernel ()
					let result = Control<Vec2>.For (0, 2, new Vec2 (0f),
						(i, v) => Vec2With (v, i, signal (v + Vec2With (new Vec2 (0f), i, dv[i])) - value)
					)
					select new CLTuple<float, Vec2> ()
					{
						Item1 = value,
						Item2 = result
					}
				)
			);

		public static readonly Macro<Macro<Vec2, float>, Macro<Vec2, float>, Vec2, float> 
			Warp = CLKernel.Macro 
			(
				() => Warp,
				(signal, warp, vec) => Kernel.Evaluate
				(
					from dv in Dv ().ToKernel ()
					let warpDv = Dfdv2 (warp, dv, vec).Item2
					select signal (vec + warpDv)
				)
			);

		public static readonly Macro<float, float, float, float> 
			Blend = CLKernel.Macro 
			(
				() => Blend,
				(signal, other, blendFactor) => FMath.Mix (signal, other, blendFactor)
			);

		public static readonly Macro<Vec3, Vec3, float, Vec3> 
			Blend3 = CLKernel.Macro 
			(
				() => Blend3,
				(signal, other, blendFactor) => signal.Mix (other, blendFactor)
			);

		public static readonly Macro<Macro<Vec2, float>, float, Vec2, CLTuple<float, Vec3>> 
			NormalMap = CLKernel.Macro 
			(
				() => NormalMap,
				(signal, strength, vec) => Kernel.Evaluate
				(
					from dv in Dv ().ToKernel ()
					let v = Dfdv2 (signal, dv, vec)
					let scale = dv * strength
					let n = new Vec3 (v.Item2 * scale, 1f).Normalized
					select new CLTuple<float, Vec3>
					{
						Item1 = v.Item1,
						Item2 = n * 0.5f + new Vec3 (0.5f)
					}
				)
			);

		public static readonly Func<Buffer<float>, Buffer<Vec4>, int, float, Vec4>
			Colorize = CLKernel.Function
			(
				() => Colorize,
				(keys, colors, count, value) => Kernel.Evaluate
				(
					from high in Control<int>.DoUntilChanges (0, count, count,
						(i, res) => (!keys)[i] > value ? i : res).ToKernel ()
					let low = high - 1
					select
						high == 0 ? (!colors)[0] :
						high == count ? (!colors)[low] :
						(!colors)[low].Mix ((!colors)[high], (value - (!keys)[low]) / 
							((!keys)[high] - (!keys)[low]))
				)
			);

		public static readonly Macro<Macro<Vec2, float>, int, int, Buffer<float>, Vec2, float>
			SpectralControl = CLKernel.Macro
			(
				() => SpectralControl,
				(signal, firstBand, lastBand, weights, vec) => 
					Control<float>.For (firstBand, lastBand + 1, 0f,
						(i, result) => Kernel.Evaluate
						(
							from input in (vec * (1 << i)).ToKernel ()
							select result + signal (input) * (!weights)[i - firstBand]
						)
					)
			);

	}
}

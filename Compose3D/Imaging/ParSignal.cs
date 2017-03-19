namespace Compose3D.Imaging
{
	using System;
	using System.Linq;
	using System.Runtime.InteropServices;
	using Cloo;
	using Extensions;
	using Compiler;
	using CLTypes;
	using Maths;
	using Parallel;

	public class PerlinArgs : ArgGroup
	{
		public readonly Value<Vec2> Scale;
		public readonly Value<int> Periodic;

		public PerlinArgs (Vec2 scale, bool periodic)
		{
			Scale = Value (scale);
			Periodic = Value (periodic ? 1 : 0);
		}
	}

	public class WorleyArgs : ArgGroup
	{
		public readonly Value<Vec2> Scale;
		public readonly Value<float> Jitter;
		public readonly Value<int> DistanceKind;
		public readonly Value<int> NoiseKind;

		public WorleyArgs (Vec2 scale, float jitter, DistanceKind distKind, WorleyNoiseKind noiseKind)
		{
			Scale = Value (scale);
			Jitter = Value (jitter);
			DistanceKind = Value ((int)distKind);
			NoiseKind = Value ((int)noiseKind);
		}
	}

	public class ColorizeArgs : ArgGroup
	{
		public readonly Buffer<float> Keys;
		public readonly Buffer<Vec4> Colors;
		public readonly Value<int> Count;

		public ColorizeArgs (ColorMap<Vec4> colorMap)
		{
			var keys = colorMap.Keys ().ToArray ();
			var colors = colorMap.Values ().ToArray ();
			Keys = Buffer (keys, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer);
			Colors = Buffer (colors, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer);
			Count = Value (keys.Length);
		}
	}

	public class SpectralControlArgs : ArgGroup
	{
		public readonly Value<int> FirstBand;
		public readonly Value<int> LastBand;
		public readonly Buffer<float> NormalizedWeights;

		public SpectralControlArgs (int firstBand, int lastBand, params float[] weights)
		{
			if (firstBand < 0)
				throw new ArgumentException ("Bands must be positive");
			if (firstBand > lastBand)
				throw new ArgumentException ("lastBand must be greater or equal to firstBand");
			if (lastBand > 15)
				throw new ArgumentException ("lastBand must be less than 16.");
			if (weights.Length != (lastBand - firstBand) + 1)
				throw new ArgumentException ("Invalid number of bands");
			var sumWeights = weights.Aggregate (0f, (s, w) => s + w);
			var normWeights = weights.Map (w => w / sumWeights);
			FirstBand = Value (firstBand);
			LastBand = Value (lastBand);
			NormalizedWeights = Buffer (normWeights, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer);
		}
	}

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
					select periodic == 0 ?
						ParPerlin.Noise (new Vec3 (scaled, 0f)) :
						ParPerlin.PeriodicNoise (new Vec3 (scaled, 0f), new Vec3 (scale, 256f))
				)
			);

		public static readonly Func<Vec2, float, int, int, Vec2, float>
			WorleyNoise = CLKernel.Function
			(
				() => WorleyNoise,
				(scale, jitter, manhattan, kind, pos) => Kernel.Evaluate
				(
					from scaled in (pos * scale).ToKernel ()
					let res = ParWorley.Noise2D (pos, jitter, manhattan)
					select
						kind == (int)WorleyNoiseKind.F1 ? res.X :
						kind == (int)WorleyNoiseKind.F2 ? res.Y :
						res.Y - res.X
				)
			);

		public static readonly Func<Vec2> 
			PixelPosTo0_1 = CLKernel.Function 
			(
				() => PixelPosTo0_1,
				() => new Vec2 (
					(float)Kernel.GetGlobalId (0) / Kernel.GetGlobalSize (0),
					(float)Kernel.GetGlobalId (1) / Kernel.GetGlobalSize (1))
			);

		public static readonly Func<Vec2> 
			Dv = CLKernel.Function 
			(
				() => Dv,
				() => new Vec2 (1f / Kernel.GetGlobalSize (0), 1f / Kernel.GetGlobalSize (1))
			);

		public static readonly Func<int> 
			PixelPosToIndex = CLKernel.Function 
			(
				() => PixelPosToIndex,
				() => Kernel.GetGlobalSize (0) * Kernel.GetGlobalId (0) + Kernel.GetGlobalId (1)
			);

		public static readonly Func<float, float> 
			NormalRangeTo0_1 = CLKernel.Function 
			(
				() => NormalRangeTo0_1,
				val => val * 0.5f + 0.5f
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
						Item2 = result / dv
					}
				)
			);

		public static readonly Macro<Macro<Vec2, float>, Macro<Vec2, float>, Vec2, Vec2, float> 
			Warp = CLKernel.Macro 
			(
				() => Warp,
				(signal, warp, dv, vec) => signal (vec + Dfdv2 (warp, dv, vec).Item2)
			);

		public static readonly Macro<Macro<Vec2, float>, Macro<Vec2, float>, float, Vec2, float> 
			Blend = CLKernel.Macro 
			(
				() => Blend,
				(signal, other, blendFactor, vec) => FMath.Mix (signal (vec), other (vec), blendFactor)
			);

		public static readonly Macro<Macro<Vec2, Vec3>, Macro<Vec2, Vec3>, float, Vec2, Vec3> 
			Blend3 = CLKernel.Macro 
			(
				() => Blend3,
				(signal, other, blendFactor, vec) => signal (vec).Mix (other (vec), blendFactor)
			);

		public static readonly Macro<Macro<Vec2, float>, Macro<Vec2, float>, Macro<Vec2, float>, Vec2, float> 
			Mask = CLKernel.Macro 
			(
				() => Mask,
				(signal, other, mask, vec) => FMath.Mix (signal (vec), other (vec), mask (vec))
			);

		public static readonly Macro<Macro<Vec2, Vec3>, Macro<Vec2, Vec3>, Macro<Vec2, float>, Vec2, Vec3> 
			Mask3 = CLKernel.Macro 
			(
				() => Mask3,
				(signal, other, mask, vec) => signal (vec).Mix (other (vec), mask (vec))
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
						(!colors)[low].Mix ((!colors)[high], (value - (!keys)[low]) / ((!keys)[high] - (!keys)[low]))
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

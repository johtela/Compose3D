namespace Compose3D.Parallel
{
	using System;
	using System.Runtime.InteropServices;
	using Compiler;
	using CLTypes;
	using Maths;

	[CLStruct]
	[StructLayout (LayoutKind.Sequential, Pack = 4)]
	public struct PerlinArgs
	{
		public Vec2 Scale;
		public int Periodic;
	}

	public static class ParSignal
	{
		public static readonly Func<float, uint> 
			FloatToUintGrayscale = CLKernel.Function 
			(
				() => FloatToUintGrayscale,
				val => Kernel.Evaluate
				(
					from c in ((uint)(val.Clamp (0f, 1f) * 255f)).ToKernel ()
					select c << 24 | c << 16 | c << 8 | 255
				)
			);

		public static readonly Func<Vec3, uint> 
			Vec3ToUintColor = CLKernel.Function 
			(
				() => Vec3ToUintColor,
				vec => Kernel.Evaluate
				(
					from h in 255f.ToKernel ()
					select (uint)(vec.X.Clamp (0f, 1f) * h) << 24 |
						(uint)(vec.Y.Clamp (0f, 1f) * h) << 16 |
						(uint)(vec.Z.Clamp (0f, 1f) * h) << 8 | 255
				)
			);

		public static readonly Func<Vec2, PerlinArgs, float> 
			Perlin = CLKernel.Function 
			(
				() => Perlin,
				(pos, args) => Kernel.Evaluate
				(
					from scaled in (pos * args.Scale).ToKernel ()
					select args.Periodic == 0 ?
						ParPerlin.Noise (new Vec3 (scaled, 0f)) :
						ParPerlin.PeriodicNoise (new Vec3 (scaled, 0f), new Vec3 (args.Scale, 256f))
				)
			);

		public static readonly Func<Vec2> 
			Pos2DTo0_1 = CLKernel.Function 
			(
				() => Pos2DTo0_1,
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
			Pos2DToIndex = CLKernel.Function 
			(
				() => Pos2DToIndex,
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

		public static readonly Macro<Macro<Vec2, float>, Vec2, Vec2, Vec2> 
			Dfdv2 = CLKernel.Macro 
			(
				() => Dfdv2,
				(signal, dv, vec) => Kernel.Evaluate
				(
					from value in signal (vec).ToKernel ()
					let result = Control<Vec2>.For (0, 2, new Vec2 (0f),
						(i, v) => Vec2With (v, i, signal (v + Vec2With (new Vec2 (0f), i, dv[i])) - value)
					)
					select result / dv
				)
			);

		public static readonly Macro<Macro<Vec2, float>, Macro<Vec2, float>, Vec2, Vec2, float> 
			Warp = CLKernel.Macro 
			(
				() => Warp,
				(signal, warp, dv, vec) => signal (vec + Dfdv2 (warp, dv, vec))
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

		public static readonly Macro<Macro<Vec2, float>, float, Vec2, Vec3> 
			NormalMap = CLKernel.Macro 
			(
				() => NormalMap,
				(signal, strength, vec) => Kernel.Evaluate
				(
					from dv in Dv ().ToKernel ()
					let v = Dfdv2 (signal, dv, vec)
					let scale = dv * strength
					let n = new Vec3 (v * scale, 1f).Normalized
					select n * 0.5f + new Vec3 (0.5f)
				)
			);

		public static CLKernel<Value<PerlinArgs>, Buffer<uint>> 
			Example = CLKernel.Create 
			(
				nameof (Example), 
				(Value<PerlinArgs> perlinArgs, Buffer<uint> result) =>
					from pos in Pos2DTo0_1 ().ToKernel ()
					let col = NormalMap (v => Perlin (v, !perlinArgs), 1f, pos)
					//let col = NormalMap (v => NormalRangeToZeroOne (Perlin (v, !perlinArgs)), 1f, pos)
					select new KernelResult
					{
						Assign.Buffer (result, Pos2DToIndex (), Vec3ToUintColor (col))
					}
			);
	}
}

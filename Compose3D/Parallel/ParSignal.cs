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
		public static readonly Func<float, uint> FloatToUintGrayscale =
			CLKernel.Function (() => FloatToUintGrayscale,
				(float signal) => Kernel.Evaluate
				(
					from c in ((uint)(signal.Clamp (0f, 1f) * 255f)).ToKernel ()
					select c << 24 | c << 16 | c << 8 | 255
				)
			);

		public static readonly Func<Vec2, PerlinArgs, float> Perlin =
			CLKernel.Function (() => Perlin,
				(Vec2 pos, PerlinArgs args) => Kernel.Evaluate
				(
					from scaled in (pos * args.Scale).ToKernel ()
					select args.Periodic == 0 ?
						ParPerlin.Noise (new Vec3 (scaled, 0f)) :
						ParPerlin.PeriodicNoise (new Vec3 (scaled, 0f), new Vec3 (args.Scale, 256f))
				)
			);

		public static readonly Func<Vec2> Pos2DToZeroOne =
			CLKernel.Function (() => Pos2DToZeroOne,
				() => Kernel.Evaluate
				(
					from x in ((float)Kernel.GetGlobalId (0)).ToKernel ()
					let y = (float)Kernel.GetGlobalId (1)
					select new Vec2 (x / Kernel.GetGlobalSize (0), y / Kernel.GetGlobalSize (1))
				)
			);

		public static readonly Func<int> Pos2DToIndex =
			CLKernel.Function (() => Pos2DToIndex,
				() => Kernel.Evaluate
				(
					from x in Kernel.GetGlobalId (0).ToKernel ()
					let y = Kernel.GetGlobalId (1)
					select Kernel.GetGlobalSize (0) * x + y
				)
			);

		public static readonly Func<float, float> NormalRangeToZeroOne =
			CLKernel.Function (() => NormalRangeToZeroOne,
				(float value) => value * 0.5f + 0.5f);

		public static readonly Macro<Macro<float, float>, float, float, float> Dfdx =
			CLKernel.Macro (() => Dfdx,
				(Macro<float, float> signal, float dx, float x) =>
					(signal (x + dx) - signal (x)) / dx
			);

		public static readonly Macro<Macro<Vec2, float>, Vec2, Vec2, Vec2> Dfdv2 =
			CLKernel.Macro (() => Dfdv2,
				(Macro<Vec2, float> signal, Vec2 dv, Vec2 v) => Kernel.Evaluate
				(
					from value in signal (v).ToKernel ()
					select new Vec2 (
						signal (v + new Vec2 (dv.X, 0f)) - value,
						signal (v + new Vec2 (0f, dv.Y)) - value) / dv
				) 
			);

		public static CLKernel<Value<PerlinArgs>, Buffer<uint>> Example =
			CLKernel.Create (nameof (Example), 
				(Value<PerlinArgs> perlinArgs, Buffer<uint> result) =>
				from pos in Pos2DToZeroOne ().ToKernel ()
				let perlin = NormalRangeToZeroOne (Perlin (pos, !perlinArgs))
				select new KernelResult
				{
					Assign.Buffer (result, Pos2DToIndex (), FloatToUintGrayscale (perlin))
				}
			);
	}
}

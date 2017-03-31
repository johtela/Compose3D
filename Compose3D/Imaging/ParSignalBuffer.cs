namespace Compose3D.Imaging
{
	using System;
	using System.Linq;
	using Cloo;
	using Extensions;
	using Compiler;
	using CLTypes;
	using Maths;

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
		public readonly Buffer<Vec2> ControlPoints;
		public readonly Value<int> Count;
		public readonly Value<int> DistanceKind;
		public readonly Value<int> NoiseKind;

		public WorleyArgs (DistanceKind distKind, WorleyNoiseKind noiseKind, params Vec2[] controlPoints)
		{
			DistanceKind = Value ((int)distKind);
			NoiseKind = Value ((int)noiseKind);
			ControlPoints = ReadBuffer (controlPoints);
			Count = Value (controlPoints.Length);
		}
	}

	public class UniformWorleyArgs : ArgGroup
	{
		public readonly Value<Vec2> Scale;
		public readonly Value<float> Jitter;
		public readonly Value<int> DistanceKind;
		public readonly Value<int> NoiseKind;

		public UniformWorleyArgs (Vec2 scale, float jitter, DistanceKind distKind, WorleyNoiseKind noiseKind)
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
			Keys = ReadBuffer (colorMap.Keys ().ToArray ());
			Colors = ReadBuffer (colorMap.Values ().ToArray ());
			Count = Value (colorMap.Count);
		}

		public ColorizeArgs (ColorMap<Vec3> colorMap)
		{
			Keys = ReadBuffer (colorMap.Keys ().ToArray ());
			Colors = ReadBuffer (colorMap.Values ().Select (v => new Vec4 (v, 1f)).ToArray ());
			Count = Value (colorMap.Count);
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
			NormalizedWeights = ReadBuffer (normWeights);
		}
	}

	public class TransformArgs : ArgGroup
	{
		public readonly Value<float> Scale;
		public readonly Value<float> Offset;

		public TransformArgs (float scale, float offset)
		{
			Scale = new Value<float> (scale);
			Offset = new Value<float> (offset);
		}
	}

	/// <summary>
	///  Buffer versions of the signal processors.
	/// </summary>
	public static class ParSignalBuffer
	{
		public static readonly Macro<Vec2i>
			GetPixelPos = CLKernel.Macro
			(
				() => GetPixelPos,
				() => new Vec2i (Kernel.GetGlobalId (0), Kernel.GetGlobalId (1))
			);

		public static readonly Macro<Vec2i>
			GetTextureSize = CLKernel.Macro
			(
				() => GetTextureSize,
				() => new Vec2i (Kernel.GetGlobalSize (0), Kernel.GetGlobalSize (1))
			);

		public static readonly Func<Vec2i, Vec2i, int>
			PixelPosToIndex = CLKernel.Function
			(
				() => PixelPosToIndex,
				(pos, size) => Kernel.Evaluate
				(
					from wrapped in new Vec2i (pos.X % size.X, pos.Y % size.Y).ToKernel ()
					select size.X * (size.Y - wrapped.Y - 1) + wrapped.X
				)
			);

		public static readonly Func<Vec2i, Vec2i, Vec2>
			PixelPosTo0_1 = CLKernel.Function
			(
				() => PixelPosTo0_1,
				(pos, size) => new Vec2 ((float)pos.X / size.X, (float)pos.Y / size.Y)
			);

		public static readonly CLKernel<PerlinArgs, Buffer<float>>
			PerlinNoise = CLKernel.Create
			(
				nameof (PerlinNoise),
				(PerlinArgs args, Buffer<float> output) =>
					from pos in GetPixelPos ().ToKernel ()
					let size = GetTextureSize ()
					let vec = PixelPosTo0_1 (pos, size)
					let val = ParSignal.PerlinNoise (!args.Scale, !args.Periodic, vec)
					select new KernelResult
					{
						Assign.Buffer (output, PixelPosToIndex (pos, size), val)
					}
			);

		public static readonly CLKernel<WorleyArgs, Buffer<float>>
			WorleyNoise = CLKernel.Create
			(
				nameof (WorleyNoise),
				(WorleyArgs args, Buffer<float> output) =>
					from pos in GetPixelPos ().ToKernel ()
					let size = GetTextureSize ()
					let vec = PixelPosTo0_1 (pos, size)
					let val = ParSignal.WorleyNoise (args.ControlPoints, !args.Count, 
						!args.DistanceKind, !args.NoiseKind, vec)
					select new KernelResult
					{
						Assign.Buffer (output, PixelPosToIndex (pos, size), val)
					}
			);

		public static readonly CLKernel<Buffer<float>, TransformArgs, Buffer<float>>
			Transform = CLKernel.Create
			(
				nameof (Transform),
				(Buffer<float> input, TransformArgs args, Buffer<float> output) =>
					from pos in GetPixelPos ().ToKernel ()
					let size = GetTextureSize ()
					let ind = PixelPosToIndex (pos, size)
					let val = ((!input)[ind] * !args.Scale + !args.Offset).Clamp (0f, 1f)
					select new KernelResult
					{
						Assign.Buffer (output, ind, val)
					}
			);

		public static readonly CLKernel<Buffer<Vec4>, Buffer<Vec4>, Value<float>, Buffer<Vec4>>
			Blend = CLKernel.Create
			(
				nameof (Blend),
				(Buffer<Vec4> input, Buffer<Vec4> other, Value<float> blendFactor, Buffer<Vec4> output) =>
					from pos in GetPixelPos ().ToKernel ()
					let size = GetTextureSize ()
					let ind = PixelPosToIndex (pos, size)
					let val = (!input)[ind].Mix ((!other)[ind], !blendFactor)
					select new KernelResult
					{
						Assign.Buffer (output, ind, val)
					}
			);

		public static readonly CLKernel<Buffer<Vec4>, Buffer<Vec4>, Buffer<float>, Buffer<Vec4>>
			Mask = CLKernel.Create
			(
				nameof (Mask),
				(Buffer<Vec4> input, Buffer<Vec4> other, Buffer<float> mask, Buffer<Vec4> output) =>
					from pos in GetPixelPos ().ToKernel ()
					let size = GetTextureSize ()
					let ind = PixelPosToIndex (pos, size)
					let val = (!input)[ind].Mix ((!other)[ind], (!mask)[ind])
					select new KernelResult
					{
						Assign.Buffer (output, ind, val)
					}
			);

		public static readonly Func<Buffer<float>, Vec2i, Vec2i, Vec2>
			Dfdv2 = CLKernel.Function
			(
				() => Dfdv2,
				(input, pos, size) => Kernel.Evaluate
				(
					from value in new Vec2 ((!input)[PixelPosToIndex (pos, size)]).ToKernel ()
					let df = new Vec2 (
						(!input)[PixelPosToIndex (new Vec2i (pos.X + 1, pos.Y), size)],
						(!input)[PixelPosToIndex (new Vec2i (pos.X, pos.Y + 1), size)])
					select df - value
				)
			);

		public static readonly CLKernel<Buffer<float>, Buffer<float>, Value<float>, Buffer<float>>
			Warp = CLKernel.Create
			(
				nameof (Warp),
				(Buffer<float> input, Buffer<float> warp, Value<float> scale, Buffer<float> output) =>
					from pos in GetPixelPos ().ToKernel ()
					let size = GetTextureSize ()
					let f = !scale * Math.Max (size.X, size.Y)
					let dv = (Dfdv2 (warp, pos, size) * f).Truncate ().ToVeci ()
					let val = (!input)[PixelPosToIndex (pos + dv, size)].Clamp (0f, 1f)
					select new KernelResult
					{
						Assign.Buffer (output, PixelPosToIndex (pos, size), val)
					}
			);

		public static readonly CLKernel<Buffer<float>, Value<float>, Buffer<Vec4>>
			NormalMap = CLKernel.Create
			(
				nameof (NormalMap),
				(Buffer<float> input, Value<float> strength, Buffer<Vec4> output) =>
					from pos in GetPixelPos ().ToKernel ()
					let size = GetTextureSize ()
					let v = (Dfdv2 (input, pos, size) * !strength).Clamp (0f, 1f)
					let n = new Vec3 (v, 1f).Normalized * 0.5f + new Vec3 (0.5f)
					select new KernelResult
					{
						Assign.Buffer (output, PixelPosToIndex (pos, size), new Vec4 (n, 1f))
					}
			);

		public static readonly CLKernel<Buffer<float>, ColorizeArgs, Buffer<Vec4>>
			Colorize = CLKernel.Create
			(
				nameof (Colorize),
				(Buffer<float> input, ColorizeArgs args, Buffer<Vec4> output) =>
					from pos in GetPixelPos ().ToKernel ()
					let size = GetTextureSize ()
					let ind = PixelPosToIndex (pos, size)
					let val = ParSignal.Colorize (args.Keys, args.Colors, !args.Count, (!input)[ind])
					select new KernelResult
					{
						Assign.Buffer (output, ind, val)
					}
			);

		public static readonly CLKernel<Buffer<float>, SpectralControlArgs, Buffer<float>>
			SpectralControl = CLKernel.Create
			(
				nameof (SpectralControl),
				(Buffer<float> input, SpectralControlArgs args, Buffer<float> output) =>
					from pos in GetPixelPos ().ToKernel ()
					let size = GetTextureSize ()
					let val = Control<float>.For (!args.FirstBand, !args.LastBand + 1, 0f,
						(i, result) => Kernel.Evaluate
						(
							from p in (pos * (1 << i)).ToKernel ()
							let v = (!input)[PixelPosToIndex (p, size)]
							select result + v * (!args.NormalizedWeights)[i - !args.FirstBand]
						)
					)
					select new KernelResult
					{
						Assign.Buffer (output, PixelPosToIndex (pos, size), val)
					}
			);
	}
}

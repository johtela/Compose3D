namespace Compose3D.Parallel
{
	using System;
	using System.Runtime.InteropServices;
	using Compiler;
	using CLTypes;
	using Maths;

	[CLStruct]
	[StructLayout (LayoutKind.Sequential, Pack = 4)]
	public struct ColorMapEntry
	{
		public float Key;
		public Vec3 Color;
	}

	public class ColorMapArg : ArgGroup
	{
		public Buffer<ColorMapEntry> Entries;
		public Value<int> Count;
	}

	public static class ParColorMap
	{
		public static readonly Func<Buffer<ColorMapEntry>, int, float, Vec3>
			ValueAt = CLKernel.Function
			(
				() => ValueAt,
				(colMap, count, value) => Kernel.Evaluate
				(
					from high in Control<int>.DoUntilChanges (0, count, count,
						(i, res) => (!colMap)[i].Key > value ? i : res).ToKernel ()
					let low = high - 1
					select 
						high == 0 ? (!colMap)[high].Color :
						high == count ? (!colMap)[low].Color :
						(!colMap)[low].Color.Mix ((!colMap)[high].Color, 
							(value - (!colMap)[low].Key) / ((!colMap)[high].Key - (!colMap)[low].Key))
				)
			);
	}
}

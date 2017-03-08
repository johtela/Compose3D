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
				(colMap, count, value) => new Vec3 (0f)
			);
	}
}

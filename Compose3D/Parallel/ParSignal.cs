namespace Compose3D.Parallel
{
	using System;
	using System.Linq;
	using System.Linq.Expressions;
	using Compiler;
	using CLTypes;
	using Maths;

	public static class ParSignal
	{
		public static Func<uint> FloatToUintGrayscale (Expression<Func<float>> signal)
		{
			return CLProgram.Function (() => FloatToUintGrayscale (signal),
				() => (from x in signal.Compile () ().ToKernel ()
					   let c = (uint)(x.Clamp (0f, 1f) * 255f)
					   select c << 24 | c << 16 | c << 8 | 255).Evaluate ());
		}

		public static readonly Func<Func<int>, int> FooTest =
			CLProgram.Function (() => FooTest,
				(Func<int> func) => func ());

		public static void Use () { }
	}
}

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
		public static Func<T, uint> FloatToUintGrayscale<T> (this Func<T, float> signal)
		{
			return CLProgram.Function (() => FloatToUintGrayscale (signal),
				(T sample) => (from x in signal (sample).ToKernel ()
							   let c = (uint)(x.Clamp (0f, 1f) * 255f)
							   select c << 24 | c << 16 | c << 8 | 255)
							.Evaluate ());
		}

		public static readonly Func<Func<int>, int> FooTest =
			CLProgram.Function (() => FooTest,
				(Func<int> func) => func ());

		public static void Use ()
		{
			var func = FloatToUintGrayscale (ParPerlin.Noise);
		}
	}
}

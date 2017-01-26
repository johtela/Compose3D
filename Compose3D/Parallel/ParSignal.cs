namespace Compose3D.Parallel
{
	using System;
	using System.Linq;
	using System.Linq.Expressions;
	using Compiler;
	using CLTypes;
	using GLTypes;
	using Maths;

	public static class ParSignal
	{
		//public static Macro<T, uint> FloatToUintGrayscale<T> (this Macro<T, float> signal)
		//{
		//return CLProgram.Function (() => FloatToUintGrayscale (signal),
		//	(T sample) => (from x in signal (sample).ToKernel ()
		//				   let c = (uint)(x.Clamp (0f, 1f) * 255f)
		//				   select c << 24 | c << 16 | c << 8 | 255)
		//				.Evaluate ());
		//}

		public static readonly Macro<Macro<int>, int> FooTest =
			GLShader.Macro (() => FooTest,
				(Macro<int> func) => func ());

		public static void Foo ()
		{
			Macro<int> bar = () => 42;
			FooTest (() => 42);
		}
	}
}

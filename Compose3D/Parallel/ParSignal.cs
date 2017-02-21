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
		public static readonly Macro<Macro<float>, uint> FloatToUintGrayscale =
			CLKernel.Macro (() => FloatToUintGrayscale,
				(Macro<float> signal) =>
					(from x in signal ().ToKernel ()
					 let c = (uint)(x.Clamp (0f, 1f) * 255f)
					 select c << 24 | c << 16 | c << 8 | 255)
				.Evaluate ());

		public static readonly Func<Vec2i, Vec2i, uint> PerlinBuffer =
			CLKernel.Function (() => PerlinBuffer,
				(Vec2i coord, Vec2i size) => 
				FloatToUintGrayscale (() => 
					ParPerlin.Noise (new Vec3 (coord.X, coord.Y, 0f) / new Vec3 (size.X, size.Y, 0f))));

		public static CLKernel<Value<Vec2i>, uint> Example =
			CLKernel.Create (nameof (Example), 
				(Value<Vec2i> size) =>
				from x in Kernel.GetGlobalId (0).ToKernel ()
				let y = Kernel.GetGlobalId (1)
				let color = PerlinBuffer (new Vec2i (x, y), !size)
				let i = Kernel.GetGlobalSize (0) * x + y
				select new BufferResult<uint> { { i, color } }
			);
	}
}

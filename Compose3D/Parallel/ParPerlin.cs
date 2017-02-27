namespace Compose3D.Parallel
{
	using System;
	using CLTypes;
	using Maths;

	public static class ParPerlin
	{
		private static readonly Func<int, int> Permutation =
			CLKernel.Function (() => Permutation,
				(int index) => Kernel.Evaluate
				(
					from con in Kernel.Constants (new
					{
						perm = new int[]
						{
							151, 160, 137, 91, 90, 15, 131, 13, 201, 95, 96, 53, 194, 233, 7, 225, 140, 36,
							103, 30, 69, 142, 8, 99, 37, 240, 21, 10, 23, 190, 6, 148, 247, 120, 234, 75, 0, 26, 197, 62, 94, 252, 219,
							203, 117, 35, 11, 32, 57, 177, 33, 88, 237, 149, 56, 87, 174, 20, 125, 136, 171, 168, 68, 175, 74, 165, 71,
							134, 139, 48, 27, 166, 77, 146, 158, 231, 83, 111, 229, 122, 60, 211, 133, 230, 220, 105, 92, 41, 55, 46,
							245, 40, 244, 102, 143, 54, 65, 25, 63, 161, 1, 216, 80, 73, 209, 76, 132, 187, 208, 89, 18, 169, 200, 196,
							135, 130, 116, 188, 159, 86, 164, 100, 109, 198, 173, 186, 3, 64, 52, 217, 226, 250, 124, 123, 5, 202, 38,
							147, 118, 126, 255, 82, 85, 212, 207, 206, 59, 227, 47, 16, 58, 17, 182, 189, 28, 42, 223, 183, 170, 213,
							119, 248, 152, 2, 44, 154, 163, 70, 221, 153, 101, 155, 167, 43, 172, 9, 129, 22, 39, 253, 19, 98, 108,
							110, 79, 113, 224, 232, 178, 185, 112, 104, 218, 246, 97, 228, 251, 34, 242, 193, 238, 210, 144, 12, 191,
							179, 162, 241, 81, 51, 145, 235, 249, 14, 239, 107, 49, 192, 214, 31, 181, 199, 106, 157, 184, 84, 204,
							176, 115, 121, 50, 45, 127, 4, 150, 254, 138, 236, 205, 93, 222, 114, 67, 29, 24, 72, 243, 141, 128, 195,
							78, 66, 215, 61, 156, 180
						}
					})
					select con.perm[index & 0xff]
				)
			);

		private static readonly Func<Vec3, Vec3> Fade =
			CLKernel.Function (() => Fade,
				(Vec3 t) => t * t * t * (t * (t * 6f - new Vec3 (15f)) + new Vec3 (10f))	);

		private static readonly Func<int, float, float, float, float> Gradient =
			CLKernel.Function (() => Gradient,
				(int hash, float x, float y, float z) => Kernel.Evaluate
				(
					from h in (hash & 15).ToKernel ()
					let u = h < 8 ? x : y
					let v = h < 4 ? y : h == 12 || h == 14 ? x : z
					select ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v)
				)
			);

		public static readonly Func<Vec3, float> Noise =
			CLKernel.Function (() => Noise,
				(Vec3 vec) => Kernel.Evaluate
				(
					from cube in vec.Floor ().ToKernel ()
					let pt = vec - cube
					let faded = Fade (pt)
					let X = (int)cube.X
					let Y = (int)cube.Y
					let Z = (int)cube.Z
					let A = Permutation (X) + Y
					let AA = Permutation (A) + Z
					let AB = Permutation (A + 1) + Z
					let B = Permutation (X + 1) + Y
					let BA = Permutation (B) + Z
					let BB = Permutation (B + 1) + Z
					select GLMath.Mix (
						GLMath.Mix (
							GLMath.Mix (
								Gradient (Permutation (AA), pt.X, pt.Y, pt.Z),
								Gradient (Permutation (BA), pt.X - 1f, pt.Y, pt.Z),
								faded.X),
							GLMath.Mix (
								Gradient (Permutation (AB), pt.X, pt.Y - 1f, pt.Z),
								Gradient (Permutation (BB), pt.X - 1f, pt.Y - 1f, pt.Z),
								faded.X),
							faded.Y),
						GLMath.Mix (
							GLMath.Mix (
								Gradient (Permutation (AA + 1), pt.X, pt.Y, pt.Z - 1f),
								Gradient (Permutation (BA + 1), pt.X - 1f, pt.Y, pt.Z - 1f),
								faded.X),
							GLMath.Mix (
								Gradient (Permutation (AB + 1), pt.X, pt.Y - 1f, pt.Z - 1f),
								Gradient (Permutation (BB + 1), pt.X - 1f, pt.Y - 1f, pt.Z - 1f),
								faded.X),
							faded.Y),
						faded.Z)
				)
			);

		public static readonly Func<Vec3, Vec3, float> PeriodicNoise =
			CLKernel.Function (() => PeriodicNoise,
				(Vec3 vec, Vec3 period) => Kernel.Evaluate
				(
					from cube in vec.Floor ().ToKernel ()
					let pt = vec - cube
					let faded = Fade (pt)
					let iv = cube.Mod (period).Floor ().ToVeci ()
					let jv = (cube + new Vec3 (1f)).Mod (period).Floor ().ToVeci ()
					let A = Permutation (iv.X)
					let AA = Permutation (A + iv.Y)
					let AB = Permutation (A + jv.Y)
					let B = Permutation (jv.X)
					let BA = Permutation (B + iv.Y)
					let BB = Permutation (B + jv.Y)
					select GLMath.Mix (
						GLMath.Mix (
							GLMath.Mix (
								Gradient (Permutation (AA + iv.Z), pt.X, pt.Y, pt.Z),
								Gradient (Permutation (BA + iv.Z), pt.X - 1f, pt.Y, pt.Z),
								faded.X),
							GLMath.Mix (
								Gradient (Permutation (AB + iv.Z), pt.X, pt.Y - 1f, pt.Z),
								Gradient (Permutation (BB + iv.Z), pt.X - 1f, pt.Y - 1f, pt.Z),
								faded.X),
							faded.Y),
						GLMath.Mix (
							GLMath.Mix (
								Gradient (Permutation (AA + jv.Z), pt.X, pt.Y, pt.Z - 1f),
								Gradient (Permutation (BA + jv.Z), pt.X - 1f, pt.Y, pt.Z - 1f),
								faded.X),
							GLMath.Mix (
								Gradient (Permutation (AB + jv.Z), pt.X, pt.Y - 1f, pt.Z - 1f),
								Gradient (Permutation (BB + jv.Z), pt.X - 1f, pt.Y - 1f, pt.Z - 1f),
								faded.X),
							faded.Y),
						faded.Z)
				)
			);
	}
}

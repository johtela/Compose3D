namespace Compose3D.Maths
{
	using System;

	public class PerlinNoise
	{
		private static int _permSize = 256;
		private static readonly int[] _refPerm = 
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
		};

		private int[] _perm;

		public PerlinNoise () { }

		public PerlinNoise (int seed)
		{
			_perm = NewPermutation (seed);
		}

		private int[] NewPermutation (int seed)
		{
			var result = new int[_permSize];
			Array.Copy (_refPerm, result, _permSize);
			Random rnd = new Random (seed);
			for (int i = 0; i < _permSize; i++)
			{
				int j = rnd.Next (_permSize);
				int temp = result[i];
				result[i] = result[j];
				result[j] = temp;
			}
			return result;
		}

		private int Permutation (int index)
		{
			return _perm == null ?
				_refPerm[index & 0xff] :
				_perm[index & 0xff];
		}

		private static float Fade (float t)
		{
			return t * t * t * (t * (t * 6f - 15f) + 10f);
		}

		private static Vec3 Fade (Vec3 vec)
		{
			return vec.Map<Vec3, float> (Fade);
		}

		private static float Gradient (int hash, float x, float y, float z)
		{
			var h = hash & 15;
			var u = h < 8 ? x : y;
			var v = h < 4 ? y : h == 12 || h == 14 ? x : z;
			return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
		}

		public float Noise (Vec3 vec)
		{
			var cube = vec.Floor ();
			var pt = vec - cube;
			var faded = Fade (pt);
			var X = (int)cube.X;
			var Y = (int)cube.Y;
			var Z = (int)cube.Z;

			int A = Permutation (X) + Y, 
				AA = Permutation (A) + Z, 
				AB = Permutation (A + 1) + Z,
				B = Permutation (X + 1) + Y, 
				BA = Permutation (B) + Z, 
				BB = Permutation (B + 1) + Z;

			return FMath.Mix (
				FMath.Mix (
					FMath.Mix ( 
						Gradient (Permutation (AA), pt.X, pt.Y, pt.Z), 
						Gradient (Permutation (BA), pt.X - 1f, pt.Y, pt.Z),
						faded.X),
					FMath.Mix ( 
						Gradient (Permutation (AB), pt.X, pt.Y - 1f, pt.Z), 
						Gradient (Permutation (BB), pt.X - 1f, pt.Y - 1f, pt.Z),
						faded.X),
					faded.Y),
				FMath.Mix (
					FMath.Mix ( 
						Gradient (Permutation (AA + 1), pt.X, pt.Y, pt.Z - 1f), 
						Gradient (Permutation (BA + 1), pt.X - 1f, pt.Y, pt.Z - 1f),
						faded.X),
					FMath.Mix ( 
						Gradient (Permutation (AB + 1), pt.X, pt.Y - 1f, pt.Z - 1f), 
						Gradient (Permutation (BB + 1), pt.X - 1f, pt.Y - 1f, pt.Z - 1f),
						faded.X),
					faded.Y),
				faded.Z);
		}

		public float PeriodicNoise (Vec3 vec, Vec3 period)
		{
			var cube = vec.Floor ();
			var pt = vec - cube;
			var faded = Fade (pt);
			var iv = cube.Mod (period).Floor ().ToVeci ();
			var jv = (cube + new Vec3 (1f)).Mod (period).Floor ().ToVeci ();
			int A = Permutation (iv.X),
				AA = Permutation (A + iv.Y),
				AB = Permutation (A + jv.Y),
				B = Permutation (jv.X),
				BA = Permutation (B + iv.Y),
				BB = Permutation (B + jv.Y);

			return FMath.Mix (
				FMath.Mix (
					FMath.Mix (
						Gradient (Permutation (AA + iv.Z), pt.X, pt.Y, pt.Z),
						Gradient (Permutation (BA + iv.Z), pt.X - 1f, pt.Y, pt.Z),
						faded.X),
					FMath.Mix (
						Gradient (Permutation (AB + iv.Z), pt.X, pt.Y - 1f, pt.Z),
						Gradient (Permutation (BB + iv.Z), pt.X - 1f, pt.Y - 1f, pt.Z),
						faded.X),
					faded.Y),
				FMath.Mix (
					FMath.Mix (
						Gradient (Permutation (AA + jv.Z), pt.X, pt.Y, pt.Z - 1f),
						Gradient (Permutation (BA + jv.Z), pt.X - 1f, pt.Y, pt.Z - 1f),
						faded.X),
					FMath.Mix (
						Gradient (Permutation (AB + jv.Z), pt.X, pt.Y - 1f, pt.Z - 1f),
						Gradient (Permutation (BB + jv.Z), pt.X - 1f, pt.Y - 1f, pt.Z - 1f),
						faded.X),
					faded.Y),
				faded.Z);
		}
	}
}
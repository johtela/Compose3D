namespace Compose3D.Maths
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Extensions;

	public static class Noise
	{
		private static int _seed = new Random ().Next ();
		private static Vec2i _primes = new Vec2i (7919999, 6691);
		private static Vec2i[] _corners = new Vec2i[]
		{
			new Vec2i (-1, 1), new Vec2i (-1, 1), new Vec2i (1, -1), new Vec2i (1, 1)
		};
		private static Vec2i[] _sides = new Vec2i[]
		{
			new Vec2i (0, -1), new Vec2i (0, 1), new Vec2i (-1, 0), new Vec2i (1, 0)
		};
		private static Vec2i[] _nexts = new Vec2i[]
		{
			new Vec2i (0, 0), new Vec2i (0, 1), new Vec2i (1, 0), new Vec2i (1, 1)
		};

		public static float Raw (Vec2i vec)
		{
			var random = new Random (vec.Dot (_primes) * _seed);
			return Convert.ToSingle (random.NextDouble ()) * 2f - 1f;
		}

		public static float Smooth (Vec2i vec)
		{
			var corners = _corners.Aggregate (0f, (res, c) => res + Raw (c + vec)) / (_corners.Length * 4f);
			var sides = _sides.Aggregate (0f, (res, s) => res + Raw (s + vec)) / (_corners.Length * 2f);
			var center = Raw (vec) / 4f;

			return corners + sides + center;
		}

		public static float Interpolated (Vec2 vec)
		{
			var intVec = new Vec2i ((int)vec.X, (int)vec.Y);
			var fracVec = vec.Fraction ();

			var vs = _nexts.Map (next => Smooth (intVec + next));
			var i1 = GLMath.CosMix (vs[0], vs[1], fracVec.X);
			var i2 = GLMath.CosMix (vs[2], vs[3], fracVec.X);

			return GLMath.CosMix (i1, i2, fracVec.Y);
		}

		public static float Noise2D (Vec2 vec, float persistence, int octaves)
		{
			var res = 0f;
			for (int i = 0; i < octaves; i++)
			{
				var frequency = 2f.Pow (i);
				var amplitude = persistence.Pow (i); 
				res += Interpolated (vec * frequency) * amplitude;
			}
			return res;
		}
	}
}
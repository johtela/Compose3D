namespace Compose3D.Geometry
{
	using System;
	using Maths;

	public class TerrainPatch<V> where V : struct, IVertex
	{
		public readonly Vec2i Start;
		public readonly Vec2i Size;
		
		public TerrainPatch (Vec2i start, Vec2i size)
		{
			Start = start;
			Size = size;
		}
		
		private static float Noise (int x, int z)
		{
			var random = new Random (x * 7919 + z * 5569);
			return Convert.ToSingle (random.NextDouble ())  * 2f - 1f;
		}
		
		private static float SmoothNoise (int x, int z)
		{
			var corners = (Noise (x - 1, z - 1) + Noise (x + 1, z - 1) + 
				Noise (x - 1, z + 1) + Noise (x + 1, z + 1)) / 16f;
			var sides = (Noise (x - 1, z) + Noise (x + 1, z) + Noise (x, z - 1) + Noise (x, z + 1)) / 8f;
			var center = Noise (x, z) / 4f;

			return corners + sides + center;
		}
		
		private static float InterpolatedNoise (float x, float y)
		{
			var intX = (int)x.Truncate ();
			var intY = (int)y.Truncate ();
			var fracX = x.Fraction ();
			var fracY = y.Fraction ();

			var v1 = SmoothNoise (intX, intY);
			var v2 = SmoothNoise (intX + 1, intY);
			var v3 = SmoothNoise (intX, intY + 1);
			var v4 = SmoothNoise (intX + 1, intY + 1);

			var	i1 = GLMath.CosMix (v1, v2, fracX);
			var	i2 = GLMath.CosMix (v3, v4, fracX);

			return GLMath.CosMix (i1, i2, fracY);
		}	
	}
}


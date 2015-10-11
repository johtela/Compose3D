namespace Compose3D.Geometry
{
    using Arithmetics;
    using System;

	public static class VertexColor
	{
		private static Random _random = new Random();

		public static IVertexColor RGB (float red, float green, float blue)
		{
			return new VertColor (new Vec3 (red, green, blue));
		}

		public static IVertexColor Black = RGB (0f, 0f, 0f);
		public static IVertexColor White = RGB (1f, 1f, 1f);
		public static IVertexColor Red = RGB (1f, 0f, 0f);
		public static IVertexColor Green = RGB (0f, 1f, 0f);
		public static IVertexColor Blue = RGB (0f, 0f, 1f);
		public static IVertexColor Random 
		{
			get 
			{
				return new VertColor (new Vec3 ((float)_random.NextDouble (), 
					(float)_random.NextDouble (), (float)_random.NextDouble ()));
			}
		}
	}
}


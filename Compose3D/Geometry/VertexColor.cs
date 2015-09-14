namespace Compose3D.Geometry
{
    using Arithmetics;
    using System;

	public static class VertexColor
	{
		private static Random _random = new Random();

		public static IVertexMaterial RGB (float red, float green, float blue)
		{
			return new VertexMaterial (new Vec3 (red, green, blue));
		}

		public static IVertexMaterial Black = RGB (0f, 0f, 0f);
		public static IVertexMaterial White = RGB (1f, 1f, 1f);
		public static IVertexMaterial Red = RGB (1f, 0f, 0f);
		public static IVertexMaterial Green = RGB (0f, 1f, 0f);
		public static IVertexMaterial Blue = RGB (0f, 0f, 1f);
		public static IVertexMaterial Random 
		{
			get 
			{
				return new VertexMaterial (new Vec3 ((float)_random.NextDouble (), 
					(float)_random.NextDouble (), (float)_random.NextDouble ()));
			}
		}
	}
}


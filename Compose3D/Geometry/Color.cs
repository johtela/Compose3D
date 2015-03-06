namespace Compose3D.Geometry
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using OpenTK;
	using OpenTK.Graphics.OpenGL;
    using Arithmetics;

	public static class Color
	{
		private static Random _random = new Random();

		public static Vec4 RGB (float red, float green, float blue)
		{
			return new Vec4 (red, green, blue, 0f);
		}

		public static Vec4 Black = new Vec4 (0f, 0f, 0f, 0f);
		public static Vec4 White = new Vec4 (1f, 1f, 1f, 0f);
		public static Vec4 Red = new Vec4 (1f, 0f, 0f, 0f);
		public static Vec4 Green = new Vec4 (0f, 1f, 0f, 0f);
		public static Vec4 Blue = new Vec4 (0f, 0f, 1f, 0f);
		public static Vec4 Random 
		{
			get 
			{
				return new Vec4 ((float)_random.NextDouble (), (float)_random.NextDouble (), (float)_random.NextDouble (), 1f);
			}
		}
	}
}


namespace Compose3D.Geometry
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using OpenTK;
	using OpenTK.Graphics.OpenGL;

	public static class Color
	{
		private static Random _random = new Random();

		public static Vector4 RGB (float red, float green, float blue)
		{
			return new Vector4 (red, green, blue, 0.0f);
		}

		public static Vector4 Black = new Vector4 (0.0f, 0.0f, 0.0f, 0.0f);
		public static Vector4 White = new Vector4 (1.0f, 1.0f, 1.0f, 0.0f);
		public static Vector4 Red = new Vector4 (1.0f, 0.0f, 0.0f, 0.0f);
		public static Vector4 Green = new Vector4 (0.0f, 1.0f, 0.0f, 0.0f);
		public static Vector4 Blue = new Vector4 (0.0f, 0.0f, 1.0f, 0.0f);
		public static Vector4 Random 
		{
			get 
			{
				return new Vector4 ((float)_random.NextDouble (), (float)_random.NextDouble (), (float)_random.NextDouble (), 1.0f);
			}
		}
	}
}


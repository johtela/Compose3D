namespace Compose3D.Geometry
{
    using Arithmetics;
    using System;
    using System.Collections.Generic;
    using System.Linq;

	public interface IVertexColor<V> where V : struct, IVec<V, float>
    {
        V Diffuse { get; set; }
        V Specular { get; set; }
		float Shininess { get; set; }
    }

	public static class VertexColor<V> where V : struct, IVec<V, float>
	{
        private static Random _random = new Random ();

		private class VertColor : IVertexColor<V>
        {
            public V Diffuse { get; set; }
            public V Specular { get; set; }
            public float Shininess { get; set; }

            public VertColor (V diffuse, V specular, float shininess)
            {
                Diffuse = diffuse;
                Specular = specular;
                Shininess = shininess;
            }

			public VertColor (V color) : this (color, color.Divide (2f), 100f) { }
        }

		public static IVertexColor<V> RGB (float red, float green, float blue)
		{
			return new VertColor (Vec.FromArray<V, float> (red, green, blue, 1f));
		}

		public static IVertexColor<V> Black = RGB (0f, 0f, 0f);
		public static IVertexColor<V> White = RGB (1f, 1f, 1f);
		public static IVertexColor<V> Red = RGB (1f, 0f, 0f);
		public static IVertexColor<V> Green = RGB (0f, 1f, 0f);
		public static IVertexColor<V> Blue = RGB (0f, 0f, 1f);
		public static IVertexColor<V> Random 
		{
			get 
			{
				return RGB ((float)_random.NextDouble (), (float)_random.NextDouble (), 
					(float)_random.NextDouble ());
			}
		}

		public static IVertexColor<V> Brass = new VertColor(
            Vec.FromArray<V, float> (0.780392f, 0.568627f, 0.113725f),
            Vec.FromArray<V, float> (0.992157f, 0.941176f, 0.807843f),
            27.8974f);

		public static IVertexColor<V> Bronze = new VertColor (
            Vec.FromArray<V, float> (0.714f, 0.4284f, 0.18144f),
            Vec.FromArray<V, float> (0.393548f, 0.271906f,	0.166721f),
            25.6f);

		public static IVertexColor<V> Chrome = new VertColor (
            Vec.FromArray<V, float> (0.4f, 0.4f, 0.4f),
            Vec.FromArray<V, float> (0.774597f, 0.774597f, 0.774597f),
            76.8f);

		public static IVertexColor<V> BlackPlastic = new VertColor (
            Vec.FromArray<V, float> (0.01f, 0.01f, 0.01f),
            Vec.FromArray<V, float> (0.5f, 0.5f, 0.5f),
            32f);
	}

	public static class ColorHelper
	{
		public static void Color<TVert, V> (this TVert[] vertices, IVertexColor<V> color)
			where TVert : struct, IVertex, IVertexColor<V>
			where V : struct, IVec<V, float>
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].Diffuse = color.Diffuse;
                vertices[i].Specular = color.Specular;
                vertices[i].Shininess = color.Shininess;
            }
        }

		public static Geometry<TVert> Color<TVert, V> (this Geometry<TVert> geometry, IVertexColor<V> color)
			where TVert : struct, IVertex, IVertexColor<V>
			where V : struct, IVec<V, float>
        {
			var result = new Wrapper<TVert> (geometry);
            Color (result.Vertices, color);
            return result;
        }
    }
}
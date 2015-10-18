namespace Compose3D.Geometry
{
    using Arithmetics;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public interface IVertexColor
    {
        Vec3 Diffuse { get; set; }
        Vec3 Specular { get; set; }
        float Shininess { get; set; }
    }

    public static class VertexColor
	{
        private static Random _random = new Random ();

        private class VertColor : IVertexColor
        {
            public Vec3 Diffuse { get; set; }
            public Vec3 Specular { get; set; }
            public float Shininess { get; set; }

            public VertColor (Vec3 diffuse, Vec3 specular, float shininess)
            {
                Diffuse = diffuse;
                Specular = specular;
                Shininess = shininess;
            }

            public VertColor (Vec3 color) : this (color, color / 2f, 100f) { }
        }

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

        public static IVertexColor Brass = new VertColor(
            new Vec3 (0.780392f, 0.568627f, 0.113725f),
            new Vec3 (0.992157f, 0.941176f, 0.807843f),
            27.8974f);

        public static IVertexColor Bronze = new VertColor (
            new Vec3 (0.714f, 0.4284f, 0.18144f),
            new Vec3 (0.393548f, 0.271906f,	0.166721f),
            25.6f);

        public static IVertexColor Chrome = new VertColor (
            new Vec3 (0.4f, 0.4f, 0.4f),
            new Vec3 (0.774597f, 0.774597f, 0.774597f),
            76.8f);

        public static IVertexColor BlackPlastic = new VertColor (
            new Vec3 (0.01f, 0.01f, 0.01f),
            new Vec3 (0.5f, 0.5f, 0.5f),
            32f);

        public static void Color<V> (this V[] vertices, IVertexColor color)
            where V : struct, IVertex, IVertexColor
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].Diffuse = color.Diffuse;
                vertices[i].Specular = color.Specular;
                vertices[i].Shininess = color.Shininess;
            }
        }

        public static Geometry<V> Color<V>(this Geometry<V> geometry, IVertexColor color)
            where V : struct, IVertex, IVertexColor
        {
			var result = new Wrapper<V> (geometry);
            Color (result.Vertices, color);
            return result;
        }
    }
}
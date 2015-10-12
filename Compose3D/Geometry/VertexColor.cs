namespace Compose3D.Geometry
{
    using Arithmetics;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public interface IVertexColor
    {
        Vec3 DiffuseColor { get; set; }
        Vec3 SpecularColor { get; set; }
        float Shininess { get; set; }
    }

    public static class VertexColor
	{
        private static Random _random = new Random ();

        private class VertColor : IVertexColor
        {
            public Vec3 DiffuseColor { get; set; }
            public Vec3 SpecularColor { get; set; }
            public float Shininess { get; set; }

            public VertColor (Vec3 diffuse, Vec3 specular, float shininess)
            {
                DiffuseColor = diffuse;
                SpecularColor = specular;
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

        public static IEnumerable<IVertexColor> Uniform (IVertexColor color)
        {
            return new IVertexColor[] { color }.Repeat ();
        }

        public static IEnumerable<IVertexColor> Uniform (Vec3 color)
        {
            return Uniform (new VertColor (color));
        }

        public static IEnumerable<IVertexColor> Repeat (params IVertexColor[] colors)
        {
            return colors.Repeat ();
        }

        public static IEnumerable<IVertexColor> Repeat (params Vec3[] colors)
        {
            return colors.Select (c => new VertColor (c)).Repeat ();
        }

        public static void Color<V>(this V[] vertices, IEnumerable<IVertexColor> colors)
            where V : struct, IVertex, IVertexColor
        {
            var colEnum = colors.GetEnumerator ();
            for (int i = 0; i < vertices.Length; i++)
            {
                var col = colEnum.Next ();
                vertices[i].DiffuseColor = col.DiffuseColor;
                vertices[i].SpecularColor = col.SpecularColor;
                vertices[i].Shininess = col.Shininess;
            }
        }

        public static Geometry<V> Color<V>(this Geometry<V> geometry, IEnumerable<IVertexColor> colors)
            where V : struct, IVertex, IVertexColor
        {
            Color (geometry.Vertices, colors);
            return geometry;
        }
    }
}



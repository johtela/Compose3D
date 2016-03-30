namespace Compose3D.Geometry
{
    using Compose3D.Maths;
    using System;
    using System.Collections.Generic;
    using System.Linq;

	public interface IDiffuseColor<V> where V : struct, IVec<V, float>
	{
		V diffuse { get; set; }
	}

	public interface ISpecularColor<V> where V : struct, IVec<V, float>
	{
		V specular { get; set; }
		float shininess { get; set; }
	}
	
	public interface IReflective
	{
		float reflectivity { get; set; }
	}

	public interface IVertexColor<V> : IDiffuseColor<V>, ISpecularColor<V>
		where V : struct, IVec<V, float>
    { }

	public static class VertexColor<V> where V : struct, IVec<V, float>
	{
        private static Random _random = new Random ();

		private class VertColor : IVertexColor<V>
        {
            public V diffuse { get; set; }
            public V specular { get; set; }
            public float shininess { get; set; }

            public VertColor (V diff, V spec, float shine)
            {
                diffuse = diff;
                specular = spec;
                shininess = shine;
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
		public static IVertexColor<V> Grey = RGB (0.5f, 0.5f, 0.5f);
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
            Vec.FromArray<V, float> (0.05f, 0.05f, 0.05f),
            Vec.FromArray<V, float> (0.6f, 0.6f, 0.6f),
            12f);

		public static IVertexColor<V> GreyPlastic = new VertColor (
			Vec.FromArray<V, float> (0.6f, 0.6f, 0.6f),
			Vec.FromArray<V, float> (0.3f, 0.3f, 0.3f),
			10f);

		public static IVertexColor<V> BluePlastic = new VertColor (
			Vec.FromArray<V, float> (0.2f, 0.2f, 0.2f),
			Vec.FromArray<V, float> (0.7f, 0.7f, 0.7f),
			72f);
	}

	public static class ColorHelper
	{
		public static void Color<TVert, V> (this TVert[] vertices, V color)
			where TVert : struct, IDiffuseColor<V> 
			where V : struct, IVec<V, float>
		{
			for (int i = 0; i < vertices.Length; i++)
				vertices[i].diffuse = color;
		}

		public static void Color<TVert, V> (this TVert[] vertices, IEnumerable<V> colors)
			where TVert : struct, IDiffuseColor<V> 
			where V : struct, IVec<V, float>
		{
			var e = colors.GetEnumerator ();
			for (int i = 0; i < vertices.Length && e.MoveNext (); i++)
				vertices [i].diffuse = e.Current;
		}

		public static void Color<TVert, V> (this TVert[] vertices, IVertexColor<V> color)
			where TVert : struct, IVertexColor<V>
			where V : struct, IVec<V, float>
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].diffuse = color.diffuse;
                vertices[i].specular = color.specular;
                vertices[i].shininess = color.shininess;
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
		
		public static void Reflectivity<TVert> (this TVert[] vertices, float reflectivity)
			where TVert : struct, IReflective
		{
			for (int i = 0; i < vertices.Length; i++)
				vertices [i].reflectivity = reflectivity;
		}		

		public static Geometry<TVert> Reflectivity<TVert> (this Geometry<TVert> geometry, float reflectivity)
			where TVert : struct, IVertex, IReflective
		{
			var result = new Wrapper<TVert> (geometry);
			Reflectivity (result.Vertices, reflectivity);
			return result;
		}
	}
}
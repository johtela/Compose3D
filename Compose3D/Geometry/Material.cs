namespace Compose3D.Geometry
{
    using Arithmetics;
    using System.Collections.Generic;
	using System.Linq;

	public interface IVertexColor
	{
		Vec3 DiffuseColor { get; set; }
		Vec3 SpecularColor { get; set; }
        float Shininess { get; set; }
	}

	/// <summary>
	/// Interface that returns the material attributes of the vertices.
	/// </summary>
	public interface IColors
	{
		/// <summary>
		/// Get the vertex colors.
		/// </summary>
		IEnumerable<IVertexColor> VertexColors { get; }
	}

	public class VertColor : IVertexColor
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

	/// <summary>
	/// Static class that contains extension methods to generate various materials,
	/// </summary>
	public static class Colors
	{
		private class ColorVec : IColors
		{
			private Vec3[] _colors;

			public ColorVec (params Vec3[] colors)
			{
				_colors = colors;
			}

			public IEnumerable<IVertexColor> VertexColors
			{
				get { return _colors.Select (v => new VertColor (v)); }
			}
		}

		private class RepeatColors : IColors
		{
			private IColors[] _colors;

			public RepeatColors (params IColors[] colors)
			{
				_colors = colors;
			}

			public IEnumerable<IVertexColor> VertexColors
			{
				get { return _colors.SelectMany (c => c.VertexColors).Repeat (); }
			}
		}

		public static IColors Uniform (Vec3 color)
		{
			return new ColorVec (color);
		}

		public static IColors Repeat (params Vec3[] colors)
		{
			return new ColorVec (colors);
		}

		public static IColors Repeat (params IColors[] colors)
		{
			return new RepeatColors (colors);
		}
	}	
}
namespace Compose3D.Geometry
{
    using Arithmetics;
    using System.Collections.Generic;
	using System.Linq;

	public interface IVertexMaterial
	{
		Vec3 DiffuseColor { get; set; }
		Vec3 SpecularColor { get; set; }
	}

	/// <summary>
	/// Interface that returns the material attributes of the vertices.
	/// </summary>
	public interface IMaterial
	{
		/// <summary>
		/// Get the vertex colors.
		/// </summary>
		IEnumerable<IVertexMaterial> VertexMaterials { get; }
	}

	public class VertexMaterial : IVertexMaterial
	{
		public Vec3 DiffuseColor { get; set; }
		public Vec3 SpecularColor { get; set; }

		public VertexMaterial (Vec3 diffuse, Vec3 specular)
		{
			DiffuseColor = diffuse;
			SpecularColor = specular;
		}

		public VertexMaterial (Vec3 color) : this (color, color) { }
	}

	/// <summary>
	/// Static class that contains extension methods to generate various materials,
	/// </summary>
	public static class Material
	{
		private class UniformColorMaterial : IMaterial
		{
			private Vec3 _color;

			public UniformColorMaterial (Vec3 color)
			{
				_color = color;
			}

			public IEnumerable<IVertexMaterial> VertexMaterials
			{
				get { while (true) yield return new VertexMaterial (_color); }
			}
		}

		public class RepeatColorsMaterial : IMaterial
		{
			private Vec3[] _colors;

			public RepeatColorsMaterial (params Vec3[] colors)
			{
				_colors = colors;
			}

			public IEnumerable<IVertexMaterial> VertexMaterials
			{
				get { return _colors.Select (c => new VertexMaterial (c)).Repeat (); }
			}
		}

		public class RepeatMaterial : IMaterial
		{
			private IMaterial[] _materials;
			private int _index;

			public RepeatMaterial (params IMaterial[] materials)
			{
				_materials = materials;
				_index = -1;
			}

			public IEnumerable<IVertexMaterial> VertexMaterials
			{
				get
				{
					_index = (_index + 1) % _materials.Length;
					return _materials[_index].VertexMaterials;
				}
			}
		}

		public static IMaterial UniformColor (Vec3 color)
		{
			return new UniformColorMaterial (color);
		}

		public static IMaterial RepeatColors (params Vec3[] colors)
		{
			return new RepeatColorsMaterial (colors);
		}

		public static IMaterial Repeat (params IMaterial[] materials)
		{
			return new RepeatMaterial (materials);
		}
	}	
}
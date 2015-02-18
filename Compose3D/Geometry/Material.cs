namespace Compose3D.Geometry
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using OpenTK;
	using OpenTK.Graphics.OpenGL;
    using GLSL;

	/// <summary>
	/// Interface that returns the material attributes of the vertices.
	/// </summary>
	public interface IMaterial
	{
		/// <summary>
		/// Get the vertex colors.
		/// </summary>
		IEnumerable<Vec4> Colors { get; }
	}

	/// <summary>
	/// Static class that contains extension methods to generate various materials,
	/// </summary>
	public static class Material
	{
		private class UniformColorMaterial : IMaterial
		{
			private Vec4 _color;

			public UniformColorMaterial (Vec4 color)
			{
				_color = color;
			}

			public IEnumerable<Vec4> Colors
			{
				get { while (true) yield return _color; }
			}
		}

		public class RepeatColorsMaterial : IMaterial
		{
			private Vec4[] _colors;

			public RepeatColorsMaterial (params Vec4[] colors)
			{
				_colors = colors;
			}

			public IEnumerable<Vec4> Colors
			{
				get { return _colors.Repeat (); }
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

			public IEnumerable<Vec4> Colors
			{
				get
				{
					_index = (_index + 1) % _materials.Length;
					return _materials[_index].Colors;
				}
			}
		}

		public static IMaterial UniformColor (Vec4 color)
		{
			return new UniformColorMaterial (color);
		}

		public static IMaterial RepeatColors (params Vec4[] colors)
		{
			return new RepeatColorsMaterial (colors);
		}

		public static IMaterial Repeat (params IMaterial[] materials)
		{
			return new RepeatMaterial (materials);
		}
	}	
}
namespace Visual3D
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using OpenTK;
	using OpenTK.Graphics.OpenGL;

	/// <summary>
	/// Interface that returns the material attributes of the vertices.
	/// </summary>
	public interface IMaterial
	{
		/// <summary>
		/// Get the vertex colors.
		/// </summary>
		IEnumerable<Vector4> Colors { get; }
	}

	/// <summary>
	/// Static class that contains extension methods to generate various materials,
	/// </summary>
	public static class Material
	{
		private class UniformColorMaterial : IMaterial
		{
			private Vector4 _color;

			public UniformColorMaterial (Vector4 color)
			{
				_color = color;
			}

			public IEnumerable<Vector4> Colors
			{
				get { while (true) yield return _color; }
			}
		}

		public class RepeatColorsMaterial : IMaterial
		{
			private Vector4[] _colors;
			private int _index;

			public RepeatColorsMaterial (params Vector4[] colors)
			{
				_colors = colors;
				_index = -1;
			}

			public IEnumerable<Vector4> Colors
			{
				get
				{
					while (true)
					{
						_index = (_index + 1) % _colors.Length;
						yield return _colors[_index];
					}
				}
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

			public IEnumerable<Vector4> Colors
			{
				get
				{
					_index = (_index + 1) % _materials.Length;
					return _materials[_index].Colors;
				}
			}
		}

		public static IMaterial UniformColor (Vector4 color)
		{
			return new UniformColorMaterial (color);
		}

		public static IMaterial RepeatColors (params Vector4[] colors)
		{
			return new RepeatColorsMaterial (colors);
		}

		public static IMaterial Repeat (params IMaterial[] materials)
		{
			return new RepeatMaterial (materials);
		}
	}	
}
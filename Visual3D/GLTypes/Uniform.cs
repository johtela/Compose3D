namespace Visual3D.GLTypes
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using OpenTK;
	using OpenTK.Graphics.OpenGL;

	public class Uniform
	{
		internal int _glUniform;

		public Uniform (int glUniform)
		{
			_glUniform = glUniform;
		}

		public static Uniform operator & (Uniform uniform, int value)
		{
			GL.Uniform1 (uniform._glUniform, value);
			return uniform;
		}

		public static Uniform operator & (Uniform uniform, uint value)
		{
			GL.Uniform1 (uniform._glUniform, value);
			return uniform;
		}

		public static Uniform operator & (Uniform uniform, float value)
		{
			GL.Uniform1 (uniform._glUniform, value);
			return uniform;
		}

		public static Uniform operator & (Uniform uniform, double value)
		{
			GL.Uniform1 (uniform._glUniform, value);
			return uniform;
		}

		public static Uniform operator & (Uniform uniform, int[] value)
		{
			GL.Uniform1 (uniform._glUniform, value.Length, value);
			return uniform;
		}

		public static Uniform operator & (Uniform uniform, uint[] value)
		{
			GL.Uniform1 (uniform._glUniform, value.Length, value);
			return uniform;
		}

		public static Uniform operator & (Uniform uniform, float[] value)
		{
			GL.Uniform1 (uniform._glUniform, value.Length, value);
			return uniform;
		}

		public static Uniform operator & (Uniform uniform, double[] value)
		{
			GL.Uniform1 (uniform._glUniform, value.Length, value);
			return uniform;
		}

		public static Uniform operator & (Uniform uniform, Matrix4 value)
		{
			GL.UniformMatrix4 (uniform._glUniform, false, ref value);
			return uniform;
		}
	}
}

namespace Visual3D.GLTypes
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using OpenTK;
	using OpenTK.Graphics.OpenGL;

	public class Uniform<T> where T : struct
	{
		private static Dictionary<Type, Action<int, object>> _setters = new Dictionary<Type, Action<int, object>> ()
		{
			{ typeof(int), (u, o) => GL.Uniform1 (u, (int)o) },
			{ typeof(uint), (u, o) => GL.Uniform1 (u, (uint)o) },
			{ typeof(float), (u, o) => GL.Uniform1 (u, (float)o) },
			{ typeof(double), (u, o) => GL.Uniform1 (u, (double)o) },
			{ typeof(int[]), (u, o) => { var a = (int[])o; GL.Uniform1 (u, a.Length, a); }},
			{ typeof(uint[]), (u, o) => { var a = (uint[])o; GL.Uniform1 (u, a.Length, a); }},
			{ typeof(float[]), (u, o) => { var a = (float[])o; GL.Uniform1 (u, a.Length, a); }},
			{ typeof(double[]), (u, o) => { var a = (double[])o; GL.Uniform1 (u, a.Length, a); }},
			{ typeof(Matrix4), (u, o) => { var m = (Matrix4)o; GL.UniformMatrix4 (u, false, ref m); }},
		};

		internal int _glUniform;

		public Uniform (int glUniform)
		{
			_glUniform = glUniform;
		}

		public static Uniform<T> operator & (Uniform<T> uniform, T value)
		{
			try
			{
				_setters[typeof (T)] (uniform._glUniform, (object)value);
				return uniform;
			}
			catch (KeyNotFoundException)
			{
				throw new GLError ("Incompatible uniform type: " + typeof (T).Name);
			}
		}
	}
}

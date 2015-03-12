namespace Compose3D.GLTypes
{
    using Arithmetics;
    using OpenTK.Graphics.OpenGL;
    using System;
    using System.Collections.Generic;

	public class Uniform<T>
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
			{ typeof(Vec3), (u, o) => GL.Uniform3 (u, 1, Vec.ToArray<Vec3, float> ((Vec3)o)) },
			{ typeof(Vec4), (u, o) => GL.Uniform4 (u, 1, Vec.ToArray<Vec4, float> ((Vec4)o)) },
			{ typeof(Mat3), (u, o) => GL.UniformMatrix3 (u, 1, false, Mat.ToArray<Mat3, float> ((Mat3)o)) },
			{ typeof(Mat4), (u, o) => GL.UniformMatrix4 (u, 1, false, Mat.ToArray<Mat4, float> ((Mat4)o)) }
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

        [GLUnaryOperator ("{0}")]
        public static T operator ! (Uniform<T> uniform)
        {
            return default (T);
        }
	}
}

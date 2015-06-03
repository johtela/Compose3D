namespace Compose3D.GLTypes
{
	using System;
	using System.Linq;
	using System.Linq.Expressions;
	using System.Reflection;
	using OpenTK.Graphics.OpenGL;
	using System.Collections.Generic;
	using System.Text;
	using System.Globalization;

	public delegate T Shader<T> (ShaderState state); 

	public static class Shader
	{
		[LiftMethod]
		public static Shader<T> ToShader<T> (this T value)
		{
			return state => value;
		}

		public static Shader<U> Bind<T, U> (this Shader<T> shader, Func<T, Shader<U>> func)
		{
			return state => func (shader (state)) (state);
		}

		public static T Execute<T> (this Shader<T> shader, ShaderState state)
		{
			return shader (state);
		}

		public static T Evaluate<T> (this Shader<T> shader)
		{
			return shader (new ShaderState (null, null));
		}

		[LiftMethod]
		public static Shader<T> Inputs<T> ()
		{
			return state => state.Inputs<T> ();
		}

		[LiftMethod]
		public static Shader<T> Uniforms<T> ()
		{
			return state => state.Uniforms<T> ();
		}

		public static Shader<U> Select<T, U> (this Shader<T> shader, Func<T, U> select)
		{
			return shader.Bind (a => select (a).ToShader ());
		}

		public static Shader<V> SelectMany<T, U, V> (this Shader<T> shader,
			Func<T, Shader<U>> project, Func<T, U, V> select)
		{
			return shader.Bind (a => project (a).Bind (b => select (a, b).ToShader ()));
		}

	}
}
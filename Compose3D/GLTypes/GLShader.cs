﻿namespace Compose3D.GLTypes
{
    using OpenTK.Graphics.OpenGL;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

	public class GLShader
	{
		internal int _glShader;

		public GLShader (int glShader)
		{
			_glShader = glShader;
		}

		public GLShader (ShaderType type, string source)
		{
			_glShader = GL.CreateShader (type);
			GL.ShaderSource (_glShader, source);
			GL.CompileShader (_glShader);
			var log = GL.GetShaderInfoLog (_glShader);
			if (log.ToUpper ().Contains ("ERROR"))
				throw new GLError (string.Format ("Shader compilation error:\n{0}", log));
		}

		public static GLShader FromFile (ShaderType type, string path)
		{
			return new GLShader (type, File.ReadAllText (path));
		}

		public static GLShader Create<T> (ShaderType type, Expression<Func<Shader<T>>> func)
		{
			var source = GLSLGenerator.CreateShader (func);
			Console.WriteLine(source);
			return new GLShader (type, source);
		}

		public static Func<TRes> Function<TRes> (Expression<Func<Func<TRes>>> member, Expression<Func<TRes>> func)
		{
			GLSLGenerator.CreateFunction ((member.Body as MemberExpression).Member, func);
			return func.Compile ();
		}

		public static Func<T1, TRes> Function<T1, TRes> (Expression<Func<Func<T1, TRes>>> member, 
			Expression<Func<T1, TRes>> func)
		{
			GLSLGenerator.CreateFunction ((member.Body as MemberExpression).Member, func);
			return func.Compile ();
		}

		public static Func<T1, T2, TRes> Function<T1, T2, TRes> (Expression<Func<Func<T1, T2, TRes>>> member, 
			Expression<Func<T1, T2, TRes>> func)
		{
			GLSLGenerator.CreateFunction ((member.Body as MemberExpression).Member, func);
			return func.Compile ();
		}

		public static Func<T1, T2, T3, TRes> Function<T1, T2, T3, TRes> (Expression<Func<Func<T1, T2, T3, TRes>>> member, 
			Expression<Func<T1, T2, T3, TRes>> func)
		{
			GLSLGenerator.CreateFunction ((member.Body as MemberExpression).Member, func);
			return func.Compile ();
		}

		public static Func<T1, T2, T3, T4, TRes> Function<T1, T2, T3, T4, TRes> (
			Expression<Func<Func<T1, T2, T3, T4, TRes>>> member, 
			Expression<Func<T1, T2, T3, T4, TRes>> func)
		{
			GLSLGenerator.CreateFunction ((member.Body as MemberExpression).Member, func);
			return func.Compile ();
		}
    }
}
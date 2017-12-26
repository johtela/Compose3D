namespace ComposeFX.Graphics.GLTypes
{
	using OpenTK.Graphics.OpenGL4;
	using System;
	using System.IO;
	using System.Linq.Expressions;
	using Shaders;
	using Compiler;

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
			if (log.ToUpper ().Contains ("ERROR:"))
				throw new GLError (string.Format ("Shader compilation error:\n{0}", log));
		}

		public static GLShader FromFile (ShaderType type, string path)
		{
			return new GLShader (type, File.ReadAllText (path));
		}

		public static GLShader Create<T> (ShaderType type, Expression<Func<Shader<T>>> func)
		{
			var source = GlslParser.CreateShader (func);
			Console.WriteLine(source);
			Console.WriteLine ("--------------------------------------------------------------------------------");
			return new GLShader (type, source);
		}

		/// <summary>
		/// Shader creation for geometry and tesselation shaders. They return multiple results 
		/// instead of one.
		/// </summary>
		public static GLShader CreateGeometryShader<V> (int vertexCount, int invocations, 
			PrimitiveType inputPrimitive, PrimitiveType outputPrimitive, Expression<Func<Shader<V[]>>> func)
			where V : PerVertexOut, new ()
		{
			var source = GlslParser.CreateGeometryShader (vertexCount, invocations, inputPrimitive, 
				outputPrimitive, func);
			Console.WriteLine (source);
			return new GLShader (ShaderType.GeometryShader, source);
		}

		public static GLShader CreateGeometryShader<V> (int vertexCount, PrimitiveType inputPrimitive,
			PrimitiveType outputPrimitive, Expression<Func<Shader<V[]>>> func)
			where V : PerVertexOut, new()
		{
			return CreateGeometryShader<V> (vertexCount, 0, inputPrimitive, outputPrimitive, func);
		}

		public static Func<TRes> Function<TRes> (Expression<Func<Func<TRes>>> member, Expression<Func<TRes>> func)
		{
			GlslParser.CreateFunction ((member.Body as MemberExpression).Member, func);
			return func.Compile ();
		}

		public static Func<T1, TRes> Function<T1, TRes> (Expression<Func<Func<T1, TRes>>> member, 
			Expression<Func<T1, TRes>> func)
		{
			GlslParser.CreateFunction ((member.Body as MemberExpression).Member, func);
			return func.Compile ();
		}

		public static Func<T1, T2, TRes> Function<T1, T2, TRes> (Expression<Func<Func<T1, T2, TRes>>> member, 
			Expression<Func<T1, T2, TRes>> func)
		{
			GlslParser.CreateFunction ((member.Body as MemberExpression).Member, func);
			return func.Compile ();
		}

		public static Func<T1, T2, T3, TRes> Function<T1, T2, T3, TRes> (Expression<Func<Func<T1, T2, T3, TRes>>> member, 
			Expression<Func<T1, T2, T3, TRes>> func)
		{
			GlslParser.CreateFunction ((member.Body as MemberExpression).Member, func);
			return func.Compile ();
		}	

		public static Func<T1, T2, T3, T4, TRes> Function<T1, T2, T3, T4, TRes> (
			Expression<Func<Func<T1, T2, T3, T4, TRes>>> member, 
			Expression<Func<T1, T2, T3, T4, TRes>> func)
		{
			GlslParser.CreateFunction ((member.Body as MemberExpression).Member, func);
			return func.Compile ();
		}

        public static Func<T1, T2, T3, T4, T5, TRes> Function<T1, T2, T3, T4, T5, TRes> (
            Expression<Func<Func<T1, T2, T3, T4, T5, TRes>>> member,
            Expression<Func<T1, T2, T3, T4, T5, TRes>> func)
        {
			GlslParser.CreateFunction ((member.Body as MemberExpression).Member, func);
            return func.Compile ();
        }

        public static Func<T1, T2, T3, T4, T5, T6, TRes> Function<T1, T2, T3, T4, T5, T6, TRes> (
            Expression<Func<Func<T1, T2, T3, T4, T5, T6, TRes>>> member,
            Expression<Func<T1, T2, T3, T4, T5, T6, TRes>> func)
        {
			GlslParser.CreateFunction ((member.Body as MemberExpression).Member, func);
            return func.Compile ();
        }

		public static Macro<TRes> Macro<TRes> (
			Expression<Func<Macro<TRes>>> member, 
			Expression<Macro<TRes>> macro)
		{
			GlslParser.CreateMacro ((member.Body as MemberExpression).Member, macro);
			return macro.Compile ();
		}

		public static Macro<T1, TRes> Macro<T1, TRes> (
			Expression<Func<Macro<T1, TRes>>> member,
			Expression<Macro<T1, TRes>> macro)
		{
			GlslParser.CreateMacro ((member.Body as MemberExpression).Member, macro);
			return macro.Compile ();
		}

		public static Macro<T1, T2, TRes> Macro<T1, T2, TRes> (
			Expression<Func<Macro<T1, T2, TRes>>> member,
			Expression<Macro<T1, T2, TRes>> macro)
		{
			GlslParser.CreateMacro ((member.Body as MemberExpression).Member, macro);
			return macro.Compile ();
		}

		public static Macro<T1, T2, T3, TRes> Macro<T1, T2, T3, TRes> (
			Expression<Func<Macro<T1, T2, T3, TRes>>> member,
			Expression<Macro<T1, T2, T3, TRes>> macro)
		{
			GlslParser.CreateMacro ((member.Body as MemberExpression).Member, macro);
			return macro.Compile ();
		}

		public static Macro<T1, T2, T3, T4, TRes> Macro<T1, T2, T3, T4, TRes> (
			Expression<Func<Macro<T1, T2, T3, T4, TRes>>> member,
			Expression<Macro<T1, T2, T3, T4, TRes>> macro)
		{
			GlslParser.CreateMacro ((member.Body as MemberExpression).Member, macro);
			return macro.Compile ();
		}

		public static Macro<T1, T2, T3, T4, T5, TRes> Macro<T1, T2, T3, T4, T5, TRes> (
			Expression<Func<Macro<T1, T2, T3, T4, T5, TRes>>> member,
			Expression<Macro<T1, T2, T3, T4, T5, TRes>> macro)
		{
			GlslParser.CreateMacro ((member.Body as MemberExpression).Member, macro);
			return macro.Compile ();
		}

		public static Macro<T1, T2, T3, T4, T5, T6, TRes> Macro<T1, T2, T3, T4, T5, T6, TRes> (
			Expression<Func<Macro<T1, T2, T3, T4, T5, T6, TRes>>> member,
			Expression<Macro<T1, T2, T3, T4, T5, T6, TRes>> macro)
		{
			GlslParser.CreateMacro ((member.Body as MemberExpression).Member, macro);
			return macro.Compile ();
		}
	}
}

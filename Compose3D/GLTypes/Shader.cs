namespace Compose3D.GLTypes
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using OpenTK.Graphics.OpenGL;

	public class Shader
	{
		internal int _glShader;

		public Shader (int glShader)
		{
			_glShader = glShader;
		}

		public Shader (ShaderType type, string source)
		{
			_glShader = GL.CreateShader (type);
			GL.ShaderSource (_glShader, source);
			GL.CompileShader (_glShader);
			var log = GL.GetShaderInfoLog (_glShader);
			if (log.Contains ("ERROR"))
				throw new GLError (string.Format ("Shader compilation error:\n{0}", log));
		}

        public static Shader FromFile (ShaderType type, string path)
        {
            return new Shader (type, File.ReadAllText (path));
        }

        private GLAttribute GetGLAttribute (MethodInfo mi)
        {
            if (mi == null)
                return null;
            var attrs = mi.GetCustomAttributes (typeof (GLAttribute), true);
            return attrs == null || attrs.Length == 0 ? null : attrs.Cast<GLAttribute> ().Single ();
        }

        public string ConvertToGLSL (object expr)
        {
            var result =
                expr.Match<MemberInitExpression, string> (mie =>
                    mie.Bindings.Select (ConvertToGLSL).SeparateWith ("\n")) ??
                expr.Match<MemberAssignment, string> (ma =>
                    string.Format ("    {0} = {1};", ma.Member.Name, ConvertToGLSL (ma.Expression))) ??
                expr.Match<BinaryExpression, string> (be =>
                {
                    var attr = GetGLAttribute (be.Method);
                    return attr == null ? null : 
                        string.Format (attr.Syntax, ConvertToGLSL (be.Left), ConvertToGLSL (be.Right));
                }) ??
                expr.Match<UnaryExpression, string> (ue =>
                {
                    var attr = GetGLAttribute (ue.Method);
                    return attr == null ? null : 
                        string.Format (attr.Syntax, ConvertToGLSL (ue.Operand));
                }) ??
                expr.Match<MethodCallExpression, string> (mc =>
                {
                    var attr = GetGLAttribute (mc.Method);
                    return attr == null ? null : 
                        string.Format (attr.Syntax, mc.Arguments.Select (ConvertToGLSL).SeparateWith (", "));
                }) ??
                expr.Match<MemberExpression, string> (me => me.Member.Name) ??
                expr.Match<ConstantExpression, string> (ce => ce.Value.ToString ())
                ?? null;
            if (result == null)
                throw new ArgumentException (string.Format ("Unknown expresion type {0}", expr));
            return result;
        }
    }
}

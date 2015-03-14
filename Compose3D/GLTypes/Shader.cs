namespace Compose3D.GLTypes
{
    using OpenTK.Graphics.OpenGL;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

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

        private static GLAttribute GetGLAttribute (MemberInfo mi)
        {
            if (mi == null)
                return null;
            var attrs = mi.GetCustomAttributes (typeof (GLAttribute), true);
            return attrs == null || attrs.Length == 0 ? null : attrs.Cast<GLAttribute> ().Single ();
        }

        private static string GetQualifiers (MemberInfo mi)
        {
            return mi.GetCustomAttributes (typeof (GLQualifierAttribute), true)
                .Cast<GLQualifierAttribute> ().Select (q => q.Syntax).SeparateWith (" ");
        }

        public static IEnumerable<FieldInfo> GetUniforms (Type type)
        {
            return from field in type.GetFields (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                   where field.FieldType.GetGenericTypeDefinition () == typeof (Uniform<>)
                   select field;
        }

        public static string DeclareUniforms (Type type)
        {
            return (from field in GetUniforms (type)
                    let glAttr = GetGLAttribute (field.FieldType.GetGenericArguments ().Single ())
                    select string.Format ("uniform {0} {1};", glAttr.Syntax, field.Name))
                    .SeparateWith ("\n");
        }
        
        public static string DeclareVariables (Type type, string varKind)
        {
            return (from field in type.GetFields (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    let typeAttr = GetGLAttribute (field.FieldType)
                    where typeAttr != null && !field.IsDefined (typeof (BuiltinAttribute), true)
                    let qualifiers = GetQualifiers (field)
                    select string.IsNullOrEmpty (qualifiers) ?
                        string.Format ("{0} {1} {2};", varKind, typeAttr.Syntax, field.Name) :
                        string.Format ("{0} {1} {2} {3};", qualifiers, varKind, typeAttr.Syntax, field.Name))
                    .SeparateWith ("\n");
        }

        public static string ExprToGLSL (object expr, params Type[] types)
        {
            var result =
                expr.Match<MemberInitExpression, string> (mie =>
                    "{\n" + mie.Bindings.Select (b => ExprToGLSL (b, types)).SeparateWith ("\n") + "\n}"
                ) ??
                expr.Match<MemberAssignment, string> (ma =>
                    string.Format ("    {0} = {1};", ma.Member.Name, ExprToGLSL (ma.Expression, types))
                ) ??
                expr.Match<BinaryExpression, string> (be =>
                {
                    var attr = GetGLAttribute (be.Method);
                    return attr == null ? null :
                        string.Format (attr.Syntax, ExprToGLSL (be.Left, types), ExprToGLSL (be.Right, types));
                }) ??
                expr.Match<UnaryExpression, string> (ue =>
                {
                    var attr = GetGLAttribute (ue.Method);
                    return attr == null ? null :
                        string.Format (attr.Syntax, ExprToGLSL (ue.Operand, types));
                }) ??
                expr.Match<MethodCallExpression, string> (mc =>
                {
                    var attr = GetGLAttribute (mc.Method);
                    if (attr == null) return null;
                    var args = mc.Method.IsStatic ? mc.Arguments : mc.Arguments.Prepend (mc.Object);
                    return string.Format (attr.Syntax, args.Select (a => ExprToGLSL (a, types)).SeparateWith (", "));
                }) ??
                expr.Match<MemberExpression, string> (me => 
                {
                    var attr = GetGLAttribute (me.Member);
                    if (attr == null) 
                    {
                        if (!types.Contains (me.Expression.Type))
                            throw new ArgumentException (string.Format ("Cannot access member of type {0}", me.Expression.Type));   
                        return me.Member.Name;
                    }
                    return string.Format (attr.Syntax, ExprToGLSL (me.Expression, types));
                })  ??
                expr.Match<NewExpression, string> (ne =>
                {
                    var attr = GetGLAttribute (ne.Constructor);
                    return attr == null ? null :
                        string.Format (attr.Syntax, ne.Arguments.Select (a => ExprToGLSL(a, types)).SeparateWith (", "));
                }) ??
                expr.Match<ConstantExpression, string> (ce => 
                    string.Format (CultureInfo.InvariantCulture, "{0}", ce.Value)
                ) ?? 
                null;
            if (result == null)
                throw new ArgumentException (string.Format ("Unsupported expresion type {0}", expr));
            return result;
        }
    }
}

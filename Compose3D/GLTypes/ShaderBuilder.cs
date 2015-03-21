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

    public class ShaderBuilder : ExpressionVisitor
    {
        private Type _shaderType;
        private StringBuilder _code;
        private bool _mainDefined;

        private ShaderBuilder (Type shaderType)
        {
            _shaderType = shaderType;
            _code = new StringBuilder ("#version 330\n");
        }

        public static string Execute<T> (IQueryable<T> shader)
        {
            var builder = new ShaderBuilder (typeof (T));
            builder.DeclareVariables (typeof (T), "out");
            builder.Visit (shader.Expression);
            builder.EndMain ();
            return builder._code.ToString ();
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
                .Cast<GLQualifierAttribute> ().Select (q => q.Qualifier).SeparateWith (" ");
        }

        public static IEnumerable<FieldInfo> GetUniforms (Type type)
        {
            return from field in type.GetFields (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                   where field.FieldType.GetGenericTypeDefinition () == typeof (Uniform<>)
                   select field;
        }

        private void DeclareUniforms (Type type)
        {
            foreach (var field in GetUniforms (type))
            {
                var glAttr = GetGLAttribute (field.FieldType.GetGenericArguments ().Single ());
                _code.AppendFormat ("uniform {0} {1};\n", glAttr.Syntax, field.Name);
            }
        }

        private void DeclareVariable (MemberInfo member, Type memberType, string varKind)
        {
            var typeAttr = GetGLAttribute (memberType);
            if (typeAttr != null && !member.IsDefined (typeof (BuiltinAttribute), true) &&
                !member.IsDefined (typeof (LocalAttribute), true))
            {
                var qualifiers = GetQualifiers (member);
                _code.AppendLine (string.IsNullOrEmpty (qualifiers) ?
                    string.Format ("{0} {1} {2};", varKind, typeAttr.Syntax, member.Name) :
                    string.Format ("{0} {1} {2} {3};", qualifiers, varKind, typeAttr.Syntax, member.Name));
            }
        }

        private void DeclareVariables (Type type, string varKind)
        {
            if (type.Name.StartsWith ("<>"))
                foreach (var prop in type.GetProperties (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                    DeclareVariable (prop, prop.PropertyType, varKind);
            else
                foreach (var field in type.GetFields (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                    DeclareVariable (field, field.FieldType, varKind);
        }

        private static bool IsSelect (MethodInfo mi)
        {
            return mi.DeclaringType == typeof (Queryable) && (mi.Name == "Select" || mi.Name == "SelectMany");
        }

        private void Declarations (NewExpression node, Type type)
        {
            var soType = type.GetGenericArguments ().Single ();
            var soKind = (ShaderObjectKind)((ConstantExpression)node.Arguments.First ()).Value;
            switch (soKind)
            {
                case ShaderObjectKind.Input:
                    DeclareVariables (soType, "in");
                    break;
                case ShaderObjectKind.Uniform:
                    DeclareUniforms (soType);
                    break;
                default:
                    throw new ArgumentException ("Unsupported shader object kind.", soKind.ToString ());
            }
        }

        private void StartMain ()
        {
            if (!_mainDefined)
            {
                _code.AppendLine ("void main ()");
                _code.AppendLine ("{");
                _mainDefined = true;
            }
        }

        private void EndMain ()
        {
            if (_mainDefined)
                _code.Append ("}");
        }

        private static string ExprToGLSL (Expression expr)
        {
            var result =
                expr.Match<BinaryExpression, string> (be =>
                {
                    var attr = GetGLAttribute (be.Method);
                    return attr == null ? null :
                        string.Format (attr.Syntax, ExprToGLSL (be.Left), ExprToGLSL (be.Right));
                }) ??
                expr.Match<UnaryExpression, string> (ue =>
                {
                    var attr = GetGLAttribute (ue.Method);
                    return attr == null ? null :
                        string.Format (attr.Syntax, ExprToGLSL (ue.Operand));
                }) ??
                expr.Match<MethodCallExpression, string> (mc =>
                {
                    var attr = GetGLAttribute (mc.Method);
                    if (attr == null) return null;
                    var args = mc.Method.IsStatic ? mc.Arguments : mc.Arguments.Prepend (mc.Object);
                    return string.Format (attr.Syntax, args.Select (a => ExprToGLSL (a)).SeparateWith (", "));
                }) ??
                expr.Match<MemberExpression, string> (me =>
                {
                    var attr = GetGLAttribute (me.Member);
                    return attr != null ?
                        string.Format (attr.Syntax, ExprToGLSL (me.Expression)) :
                        me.Member.Name;
                }) ??
                expr.Match<NewExpression, string> (ne =>
                {
                    var attr = GetGLAttribute (ne.Constructor);
                    return attr == null ? null :
                        string.Format (attr.Syntax, ne.Arguments.Select (a => ExprToGLSL (a)).SeparateWith (", "));
                }) ??
                expr.Match<ConstantExpression, string> (ce =>
                    string.Format (CultureInfo.InvariantCulture, "{0}", ce.Value)
                ) ?? null;
            if (result == null)
                throw new ArgumentException (string.Format ("Unsupported expression type {0}", expr));
            return result;
        }

        protected override Expression VisitNew (NewExpression node)
        {
            var createdType = node.Constructor.DeclaringType;

            if (createdType.IsGenericType && createdType.GetGenericTypeDefinition () == typeof (ShaderObject<>))
                Declarations (node, createdType);
            else if (node.Members != null)
            {
                StartMain ();
                for (int i = 0; i < node.Members.Count; i++)
                {
                    var prop = (PropertyInfo)node.Members[i];
                    if (!prop.Name.StartsWith ("<>"))
                    {
                        if (createdType == _shaderType)
                            _code.AppendFormat ("    {0} = {1};\n", prop.Name, ExprToGLSL (node.Arguments[i]));
                        else
                        {
                            var attr = GetGLAttribute (prop.PropertyType);
                            var type = attr == null ? TypeMapping.Type (prop.PropertyType) : attr.Syntax;
                            _code.AppendFormat ("    {0} {1} = {2};\n", type, prop.Name, ExprToGLSL (node.Arguments[i]));
                        }
                    }
                }
            }
            return node;
        }

        protected override Expression VisitMethodCall (MethodCallExpression node)
        {
            if (IsSelect (node.Method))
            {
                Visit (node.Arguments[0]);
                Visit (node.Arguments[1]);
            }
            return node;
        }

        protected override MemberAssignment VisitMemberAssignment (MemberAssignment node)
        {
            StartMain ();
            _code.AppendFormat ("    {0} = {1};\n", node.Member.Name, ExprToGLSL (node.Expression));
            return node;
        }
    }
}

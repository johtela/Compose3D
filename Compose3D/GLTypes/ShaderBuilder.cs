﻿namespace Compose3D.GLTypes
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using OpenTK.Graphics.OpenGL;
    using System.Collections.Generic;
    using System.Text;
    using System.Globalization;

    public class ShaderBuilder
    {
        private Type _shaderType;
        private StringBuilder _code;
        private bool _mainDefined;
        private HashSet<Type> _structsDefined;

        private ShaderBuilder (Type shaderType)
        {
            _shaderType = shaderType;
            _code = new StringBuilder ("#version 330\n");
            _structsDefined = new HashSet<Type> ();
        }

        public static string Execute<T> (IQueryable<T> shader)
        {
            var builder = new ShaderBuilder (typeof (T));
            builder.DeclareVariables (typeof (T), "out");
            var parser = builder.ShaderParser ();
            parser.Execute (new Source (shader.Expression.Traverse ()));
            return builder._code.ToString ();
        }

        private void OutputStruct (Type structType)
        {
            if (!_structsDefined.Contains (structType))
            {
                foreach (var field in structType.GetGLFields ())
                    if (field.FieldType.IsGLStruct ())
                        OutputStruct (field.FieldType);
                _structsDefined.Add (structType);
                _code.AppendFormat ("struct {0}\n{{\n", structType.Name);
                foreach (var field in structType.GetGLFields ())
                    DeclareVariable (field, field.FieldType, "   ");
                _code.AppendLine ("};");
            }
        }

        private void DeclareUniforms (Type type)
        {
            foreach (var field in type.GetUniforms ())
            {
                var uniType = field.FieldType.GetGenericArguments ().Single ();
                var arrayLen = 0;
                if (uniType.IsArray)
                {
                    var arrAttr = field.GetCustomAttributes (true).SingleOrDefault (attr => attr is GLArrayAttribute);
                    if (arrAttr == null)
                        throw new ArgumentException ("Missing GLArray attribute for array uniform.");
                    arrayLen = (arrAttr as GLArrayAttribute).Length;
                    uniType = uniType.GetElementType ();
                }
                var glAttr = uniType.GetGLAttribute ();
                if (glAttr == null)
                    throw new ArgumentException ("Unsupported uniform type: " + uniType.Name);
                if (glAttr is GLStruct)
                    OutputStruct (uniType);
                _code.AppendFormat ("uniform {0} {1}{2};\n", glAttr.Syntax, field.Name, 
                    arrayLen > 0 ? "[" + arrayLen + "]" : "");
            }
        }

        private void DeclareVariable (MemberInfo member, Type memberType, string prefix)
        {
            var typeAttr = memberType.GetGLAttribute ();
            if (typeAttr != null && !member.IsBuiltin ())
            {
                var qualifiers = member.GetQualifiers ();
                _code.AppendLine (string.IsNullOrEmpty (qualifiers) ?
                    string.Format ("{0} {1} {2};", prefix, typeAttr.Syntax, member.Name) :
                    string.Format ("{0} {1} {2} {3};", qualifiers, prefix, typeAttr.Syntax, member.Name));
            }
        }

        private void DeclareVariables (Type type, string prefix)
        {
            if (type.Name.StartsWith ("<>"))
                foreach (var prop in type.GetGLProperties ())
                    DeclareVariable (prop, prop.PropertyType, prefix);
            else
                foreach (var field in type.GetGLFields ())
                    DeclareVariable (field, field.FieldType, prefix);
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
                    var attr = be.Method.GetGLAttribute ();
                    return attr == null ? null :
                        string.Format ("("+ attr.Syntax + ")", ExprToGLSL (be.Left), ExprToGLSL (be.Right));
                }) ??
                expr.Match<UnaryExpression, string> (ue =>
                {
                    var attr = ue.Method.GetGLAttribute ();
                    return attr == null ? null :
                        string.Format (attr.Syntax, ExprToGLSL (ue.Operand));
                }) ??
                expr.Match<MethodCallExpression, string> (mc =>
                {
                    var attr = mc.Method.GetGLAttribute ();
                    if (attr == null) return null;
                    var args = mc.Method.IsStatic ? mc.Arguments : mc.Arguments.Prepend (mc.Object);
                    return string.Format (attr.Syntax, args.Select (a => ExprToGLSL (a)).SeparateWith (", "));
                }) ??
                expr.Match<MemberExpression, string> (me =>
                {
                    var attr = me.Member.GetGLAttribute ();
                    return attr != null ?
                        string.Format (attr.Syntax, ExprToGLSL (me.Expression)) :
                        me.Expression.Type.IsGLStruct () ?
                            string.Format ("{0}.{1}", ExprToGLSL (me.Expression), me.Member.Name) :
                            me.Member.Name;
                }) ??
                expr.Match<NewExpression, string> (ne =>
                {
                    var attr = ne.Constructor.GetGLAttribute ();
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

        public bool Declaration (Source source)
        {
            var node = source.Current.CastExpr<NewExpression> (ExpressionType.New);
            if (node == null)
                return false;
            var createdType = node.Constructor.DeclaringType;
            if (createdType.IsGenericType && createdType.GetGenericTypeDefinition () == typeof (ShaderObject<>))
            {
                Declarations (node, createdType);
                return true;
            }
            else return false;
        }

        public bool LetBinding (Source source)
        {
            var node = source.Current.CastExpr<NewExpression> (ExpressionType.New);
            if (node == null)
                return false;
            var createdType = node.Constructor.DeclaringType;
            if (createdType != _shaderType)
            {
                for (int i = 0; i < node.Members.Count; i++)
                {
                    var prop = (PropertyInfo)node.Members[i];
                    if (!prop.Name.StartsWith ("<>"))
                    {
                        var attr = prop.PropertyType.GetGLAttribute ();
                        var type = attr == null ? TypeMapping.Type (prop.PropertyType) : attr.Syntax;
                        if (ParseAggregate (node.Arguments[i]))
                        {
                            // TODO: something
                        }
                        else
                            _code.AppendFormat ("    {0} {1} = {2};\n", type, prop.Name, 
                                ExprToGLSL (node.Arguments[i]));
                    }
                }
                return true;
            }
            else return false;
        }

        public bool MemberInitReturn (Expression expr)
        {
            var mie = expr.CastExpr<MemberInitExpression> (ExpressionType.MemberInit);
            if (mie == null || mie.Type != _shaderType)
                return false;
            foreach (MemberAssignment assign in mie.Bindings)
                _code.AppendFormat ("    {0} = {1};\n", assign.Member.Name, ExprToGLSL (assign.Expression));
            return true;
        }

        public bool Return (Source source)
        {
            var ne = source.Current.CastExpr<NewExpression> (ExpressionType.New);
            if (ne == null)
                return MemberInitReturn (source.Current);
            if (ne.Type != _shaderType)
                return false;
            for (int i = 0; i < ne.Members.Count; i++)
            {
                var prop = (PropertyInfo)ne.Members[i];
                if (!prop.Name.StartsWith ("<>"))
                    _code.AppendFormat ("    {0} = {1};\n", prop.Name, ExprToGLSL (ne.Arguments[i]));
            }
            return true;
        }

        public bool ParseAggregate (Expression expr)
        {
            var node = expr.CastExpr<MethodCallExpression> (ExpressionType.Call);
            if (node == null)
                return false;
            if (node.Method.IsAggregate ())
            {
                if (node.Method.GetGenericArguments ().Length != 2)
                    throw new ParseException ("The only supported overload of Aggregate is one containing two generic parameters.");
                return true;
            }
            else return false;
        }

        public Parser ShaderParser ()
        {
            var decl = Parse.OneOrMore (Declaration).IfFail (new ParseException (
                "Must have at least one from clause in the beginning of Linq expression."));
            var body = Parse.ZeroOrMore (LetBinding);
            var ret = Parse.ExactlyOne (Return).IfFail (new ParseException (
                "Must have exactly one select clause at the end of Linq expression."));
            return decl.IfSucceed (StartMain).Then (body).Then (ret).IfSucceed (EndMain);
        }
    }
}

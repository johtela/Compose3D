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

    public class ShaderBuilder
    {
        private StringBuilder _code;
        private bool _mainDefined;
        private HashSet<Type> _structsDefined;
        private int _localVarCount;

        private ShaderBuilder ()
        {
            _code = new StringBuilder ("#version 330\n");
            _structsDefined = new HashSet<Type> ();
        }

        public static string Execute<T> (IQueryable<T> shader)
        {
            var builder = new ShaderBuilder ();
            builder.DeclareVariables (typeof (T), "out");
            builder.Shader (shader.Expression);
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
                    var arrAttr = field.GetGLArrayAttribute ();
                    if (arrAttr == null)
                        throw new ArgumentException ("Missing GLArray attribute for array uniform.");
                    arrayLen = arrAttr.Length;
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
            var syntax = GLType (memberType); 
            if (!member.IsBuiltin ())
            {
                var qualifiers = member.GetQualifiers ();
                _code.AppendLine (string.IsNullOrEmpty (qualifiers) ?
                    string.Format ("{0} {1} {2};", prefix, syntax, member.Name) :
                    string.Format ("{0} {1} {2} {3};", qualifiers, prefix, syntax, member.Name));
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

        private string GLType (Type type)
        {
            var attr = type.GetGLAttribute ();
            return attr == null ? TypeMapping.Type (type) : attr.Syntax;
        }

        private string NewLocalVar (string name)
        {
            return string.Format ("_gen_{0}{1}", name, ++_localVarCount);
        }

        private string ExprToGLSL (Expression expr)
        {
            var result =
                expr.Match<BinaryExpression, string> (be =>
                {
                    var attr = be.Method.GetGLAttribute ();
                    return "(" + string.Format (
                        attr != null ? attr.Syntax : TypeMapping.Operators (be.NodeType),
                        ExprToGLSL (be.Left), ExprToGLSL (be.Right)) + ")";

                }) ??
                expr.Match<UnaryExpression, string> (ue =>
                {
                    var attr = ue.Method.GetGLAttribute ();
                    return string.Format (attr != null ? attr.Syntax : TypeMapping.Operators (ue.NodeType), 
                        ExprToGLSL (ue.Operand));
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
                ) ?? 
                expr.Match<ConditionalExpression, string> (ce => string.Format ("{0} ? {1} : {2}", 
                    ExprToGLSL (ce.Test), ExprToGLSL (ce.IfTrue), ExprToGLSL (ce.IfFalse))
                ) ?? 
                expr.Match<ParameterExpression, string> (pe => 
                    pe.Name
                ) ?? null;
            if (result == null)
                throw new ArgumentException (string.Format ("Unsupported expression type {0}", expr));
            return result;
        }

        public bool Declaration (Source source)
        {
            var arg0 = source.Current.Arguments[0].CastExpr<NewExpression> (ExpressionType.New);
            var arg1 = source.Current.GetSelectLambda ().Body.Expect<NewExpression> (ExpressionType.New);
            return arg0 != null ?
                OutputDeclarations (arg0) | OutputDeclarations (arg1) :
                OutputDeclarations (arg1);
        }

        private bool OutputDeclarations (NewExpression ne)
        {
            var createdType = ne.Constructor.DeclaringType;
            if (!createdType.IsGenericType || createdType.GetGenericTypeDefinition () != typeof (ShaderObject<>))
                return false;
            Declarations (ne, createdType);
            return true;
        }

        public bool LetBinding (Source source)
        {
            return source.ParseLambda ((_, ne) => OutputLet (ne));
        }

        private bool OutputLet (NewExpression ne)
        {
            for (int i = 0; i < ne.Members.Count; i++)
            {
                var prop = (PropertyInfo)ne.Members[i];
                if (!prop.Name.StartsWith ("<>"))
                {
                    var type = GLType (prop.PropertyType);
                    var aggr = Aggregate (ne.Arguments[i]);
                    if (aggr != null)
                        _code.AppendFormat ("    {0} {1} = {2};\n", type, prop.Name, aggr);
                    else
                    {
                        var val = ExprToGLSL (ne.Arguments[i]);
                        if (prop.Name != val)
                            _code.AppendFormat ("    {0} {1} = {2};\n", type, prop.Name, val);
                    }
                }
            }
            return true;
        }

        public string Aggregate (Expression expr)
        {
            var node = expr.CastExpr<MethodCallExpression> (ExpressionType.Call);
            if (node == null || !node.Method.IsAggregate ())
                return null;
            var aggrFun = node.Arguments[2].Expect<UnaryExpression> (ExpressionType.Quote).Operand
                .Expect<LambdaExpression> (ExpressionType.Lambda);
            var accum = aggrFun.Parameters[0];
            var iterVar = aggrFun.Parameters[1];
            _code.AppendFormat ("    {0} {1} = {2};\n", GLType(accum.Type), accum.Name, 
                ExprToGLSL (node.Arguments[1]));
            var se = node.Arguments[0].ExpectSelect ();
            ForParser ().Execute (new Source (se.Arguments[0].Traverse ()));
            _code.AppendFormat ("        {0} {1} = {2};\n", GLType (iterVar.Type), iterVar.Name, 
                ExprToGLSL (se.Arguments[1].ExpectQuotedLambda ().Body));
            _code.AppendFormat ("        {0} = {1};\n", accum.Name, ExprToGLSL (aggrFun.Body));
            _code.AppendLine ("    }");
            return accum.Name;
        }

        public bool ForLoop (Source source)
        {
            var ne = source.Current.Arguments[0].CastExpr<NewExpression> (ExpressionType.New);
            if (ne == null)
                return false;
            var soType = ne.Constructor.DeclaringType;
            if (!soType.IsGenericType || soType.GetGenericTypeDefinition () != typeof (ShaderObject<>))
                return false;
            var array = ne.Arguments[0];
            var field = array.SkipUnary (ExpressionType.Not)
                .Expect<MemberExpression> (ExpressionType.MemberAccess).Member as FieldInfo;
            var attr = field.GetGLArrayAttribute ();
            if (field == null || attr == null)
                throw new ParseException ("Invalid shader object parameter. " +
                    "Expected field with GLArray attribute. Encountered: " + array);
            var indexVar = NewLocalVar ("ind");
            var le = source.Current.GetSelectLambda ();
            var item = le.Parameters[0];
            _code.AppendFormat ("    for (int {0} = 0; {0} < {1}; {0}++)\n", indexVar, attr.Length);
            _code.AppendLine ("    {");
            _code.AppendFormat ("        {0} {1} = {2}[{3}];\n", GLType (item.Type), item.Name, 
                ExprToGLSL (array), indexVar);
            OutputLet (le.Body.Expect<NewExpression> (ExpressionType.New));
            return true;
        }

        public Parser ForParser ()
        {
            return Parse.ExactlyOne (ForLoop).IfFail (new ParseException (
                "Must have exactly one from clause in the beginning of aggregate expression."))
                .Then (Parse.ZeroOrMore (LetBinding));
        }

        public void Return (Expression expr)
        {
            var ne = expr.CastExpr<NewExpression> (ExpressionType.New);
            if (ne == null)
            {
                var mie = expr.CastExpr<MemberInitExpression> (ExpressionType.MemberInit);
                if (mie == null)
                    throw new ParseException ("Unsupported shader expression: " + expr);
                foreach (MemberAssignment assign in mie.Bindings)
                    _code.AppendFormat ("    {0} = {1};\n", assign.Member.Name, ExprToGLSL (assign.Expression));
            }
            else
            {
                for (int i = 0; i < ne.Members.Count; i++)
                {
                    var prop = (PropertyInfo)ne.Members[i];
                    if (!prop.Name.StartsWith ("<>"))
                        _code.AppendFormat ("    {0} = {1};\n", prop.Name, ExprToGLSL (ne.Arguments[i]));
                }
            }
        }

        public void Shader (Expression expr)
        {
            var mce = expr.ExpectSelect ();
            var parser = ShaderParser ();
            parser.Execute (new Source (mce.Arguments[0].Traverse ()));
            Return (mce.Arguments[1].ExpectQuotedLambda ().Body);
            EndMain ();
        }

        public Parser ShaderParser ()
        {
            var decl = Parse.OneOrMore (Declaration).IfFail (new ParseException (
                "Must have at least one from clause in the beginning of Linq expression."));
            var body = Parse.ZeroOrMore (LetBinding);
            return decl.IfSucceed (StartMain).Then (body);
        }
    }
}

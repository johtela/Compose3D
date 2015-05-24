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
	using Compose3D;

    public class ShaderBuilder
    {
		internal static Dictionary<MethodInfo, string> _functions = new Dictionary<MethodInfo, string> ();
        private StringBuilder _code;
        private bool _mainDefined;
        private HashSet<Type> _structsDefined;
		private HashSet<MethodInfo> _funcRefs;
        private int _localVarCount;
        private int _tabLevel;

        private ShaderBuilder ()
        {
			_code = new StringBuilder ("#version 300 es\nprecision highp float;\n");
            _structsDefined = new HashSet<Type> ();
			_funcRefs = new HashSet<MethodInfo> ();
        }

        public static string Execute<T> (IQueryable<T> shader)
        {
            var builder = new ShaderBuilder ();
            builder.DeclareVariables (typeof (T), "out");
            builder.OutputShader (shader.Expression);
			builder.OutputReferredFunctions ();
            return builder._code.ToString ();
        }

		public static void CreateFunction<TPar, TRes> (MethodInfo mi, IQueryable<TRes> body)
		{
			if (_functions.ContainsKey (mi))
				throw new ArgumentException ("Function already declared", "mi");
			if (!mi.IsStatic)
				throw new ArgumentException ("Function method must be static", "mi");
			var builder = new ShaderBuilder ();
			builder.StartFunction (mi.Name, typeof (TPar), typeof(TRes));
			builder.FunctionBody (body.Expression);
			builder.EndFunction ();
			_functions.Add (mi, builder._code.ToString ());
		}

		private void OutputReferredFunctions ()
		{
			foreach (var mi in _funcRefs) 
				CodeOut ("\n" + _functions[mi]);
		}
        
        private string Tabs ()
        {
            var sb = new StringBuilder ();
            for (int i = 0; i < _tabLevel; i++)
                sb.Append ("    ");
            return sb.ToString ();
        }

        private void CodeOut (string code)
        {
            _code.AppendLine (Tabs () + code);
        }

        private void CodeOut (string code, params object[] args)
        {
            _code.AppendFormat (Tabs () + code + "\n", args);
        }

        private void OutputStruct (Type structType)
        {
            if (!_structsDefined.Contains (structType))
            {
                foreach (var field in structType.GetGLFields ())
                    if (field.FieldType.IsGLStruct ())
                        OutputStruct (field.FieldType);
                _structsDefined.Add (structType);
                CodeOut ("struct {0}\n{{", structType.Name);
                foreach (var field in structType.GetGLFields ())
                    DeclareVariable (field, field.FieldType, "    ");
                CodeOut ("};");
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
                    var arrAttr = field.ExpectGLArrayAttribute ();
                    arrayLen = arrAttr.Length;
                    uniType = uniType.GetElementType ();
                }
                var glAttr = uniType.GetGLAttribute ();
                if (glAttr == null)
                    throw new ArgumentException ("Unsupported uniform type: " + uniType.Name);
                if (glAttr is GLStruct)
                    OutputStruct (uniType);
				CodeOut ("uniform {0} {1}{2};", glAttr.Syntax, field.Name, 
                    arrayLen > 0 ? "[" + arrayLen + "]" : "");
            }
        }

        private void DeclareVariable (MemberInfo member, Type memberType, string prefix)
        {
            var syntax = GLType (memberType); 
            if (!member.IsBuiltin ())
            {
                var qualifiers = member.GetQualifiers ();
                CodeOut (string.IsNullOrEmpty (qualifiers) ?
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

		private string FunctionParams (Type parType)
		{
			if (parType.IsValueType)
				return GLType (parType);
			return (from field in parType.GetGLFields ()
			        select GLType (field.FieldType)).SeparateWith (", ");
		}

		private void StartFunction (string name, Type parType, Type resType)
		{
			CodeOut ("{0} {1} ({2})", GLType (resType), name, FunctionParams (parType));
			CodeOut ("{");
			_tabLevel++;
		}

		private void EndFunction ()
		{
			_tabLevel--;
			CodeOut ("}");
		}

        private void StartMain ()
        {
            if (!_mainDefined)
            {
                CodeOut ("void main ()");
                CodeOut ("{");
                _tabLevel++;
                _mainDefined = true;
            }
        }

        private void EndMain ()
        {
			if (_mainDefined)
				EndFunction ();
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
					if (attr != null) 
					{
						var args = mc.Method.IsStatic ? mc.Arguments : mc.Arguments.Prepend (mc.Object);
						return string.Format (attr.Syntax, args.Select (a => ExprToGLSL (a)).SeparateWith (", "));
					}
					if (_functions.ContainsKey (mc.Method))
					{
						_funcRefs.Add (mc.Method);
						return string.Format ("{0} ({1})", mc.Method.Name,
							mc.Arguments.Select (a => ExprToGLSL (a)).SeparateWith (", "));
					}
					return null;
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
						string.Format (CultureInfo.InvariantCulture, "{0}{1}", ce.Value, ce.Type == typeof(float) ? "f" : "")
                ) ?? 
                expr.Match<ConditionalExpression, string> (ce => string.Format ("({0} ? {1} : {2})", 
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
                        CodeOut ("{0} {1} = {2};", type, prop.Name, aggr);
                    else
                    {
                        var val = ExprToGLSL (ne.Arguments[i]);
                        if (prop.Name != val)
                            CodeOut ("{0} {1} = {2};", type, prop.Name, val);
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
            CodeOut ("{0} {1} = {2};", GLType(accum.Type), accum.Name, 
                ExprToGLSL (node.Arguments[1]));
            var se = node.Arguments[0].ExpectSelect ();
            ForParser ().Execute (new Source (se.Arguments[0].Traverse ()));
            CodeOut ("{0} {1} = {2};", GLType (iterVar.Type), iterVar.Name, 
                ExprToGLSL (se.Arguments[1].ExpectQuotedLambda ().Body));
            CodeOut ("{0} = {1};", accum.Name, ExprToGLSL (aggrFun.Body));
            _tabLevel--;
            CodeOut ("}");
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
            if (field == null)
                throw new ParseException ("Invalid shader object parameter. " +
                    "Expected field reference. Encountered: " + array);
            var attr = field.ExpectGLArrayAttribute ();
            var indexVar = NewLocalVar ("ind");
            var le = source.Current.GetSelectLambda ();
            var item = le.Parameters[0];
            CodeOut ("for (int {0} = 0; {0} < {1}; {0}++)", indexVar, attr.Length);
            CodeOut ("{");
            _tabLevel++;
            CodeOut ("{0} {1} = {2}[{3}];", GLType (item.Type), item.Name, 
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
                    CodeOut ("{0} = {1};", assign.Member.Name, ExprToGLSL (assign.Expression));
            }
            else
            {
                for (int i = 0; i < ne.Members.Count; i++)
                {
                    var prop = (PropertyInfo)ne.Members[i];
                    if (!prop.Name.StartsWith ("<>"))
                        CodeOut ("{0} = {1};", prop.Name, ExprToGLSL (ne.Arguments[i]));
                }
            }
        }

        public void OutputShader (Expression expr)
        {
            var mce = expr.ExpectSelect ();
            var ne = mce.Arguments[0].CastExpr<NewExpression> (ExpressionType.New);
            if (ne != null)
            {
                OutputDeclarations (ne);
                StartMain ();
            } else
                ShaderParser ().Execute (new Source (mce.Arguments[0].Traverse ()));
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

		public void FunctionBody (Expression expr)
		{
			var mce = expr.ExpectSelect ();
			var ne = mce.Arguments[0].CastExpr<NewExpression> (ExpressionType.New);
			if (ne == null)
				Parse.ZeroOrMore (LetBinding).Execute (new Source (mce.Arguments[0].Traverse ()));
			CodeOut ("return " + ExprToGLSL (mce.Arguments[1].ExpectQuotedLambda ().Body));
		}
    }
}
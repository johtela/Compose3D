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
	using Compose3D;

	public class GLSLGenerator
    {
		internal static Dictionary<MemberInfo, string> _functions = new Dictionary<MemberInfo, string> ();
        private StringBuilder _code;
        private bool _mainDefined;
		private int _mainStart;
        private HashSet<Type> _structsDefined;
		private HashSet<MemberInfo> _funcRefs;
        private int _localVarCount;
        private int _tabLevel;

		private GLSLGenerator (bool fragment)
        {
			_code = new StringBuilder (fragment ? "" : "#version 300 es\nprecision highp float;\n");
            _structsDefined = new HashSet<Type> ();
			_funcRefs = new HashSet<MemberInfo> ();
        }

		public static string CreateShader<T> (Expression<Func<Shader<T>>> shader)
        {
			var builder = new GLSLGenerator (false);
            builder.DeclareVariables (typeof (T), "out");
            builder.OutputShader (shader);
			builder._code.Insert (builder._mainStart, GenerateFunctions (builder._funcRefs));
            return builder._code.ToString ();
        }

		public static void CreateFunction (MemberInfo member, LambdaExpression expr)
		{
			var builder = new GLSLGenerator (true);
			builder.OutputFunction (member.Name, expr);
			_functions.Add (member, builder._code.ToString ());
		}

		public static string GenerateFunctions (HashSet<MemberInfo> functions)
		{
			if (functions.Count == 0)
				return "";
			var sb = new StringBuilder ();
			sb.AppendLine ();
			foreach (var mi in functions)
				sb.AppendLine (_functions [mi]);
			return sb.ToString ();
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

		private void Declarations (MethodCallExpression node)
        {
			var type = node.Method.GetGenericArguments () [0];
			if (node.Method.Name == "Inputs")
				DeclareVariables (type, "in");
			else if (node.Method.Name == "Uniforms")
				DeclareUniforms (type);
			else
				throw new ArgumentException ("Unknown declaration method.", node.Method.ToString ());
        }

		private void OutputFunction (string name, LambdaExpression expr)
		{
			var pars = (from p in expr.Parameters
			            select string.Format ("{0} {1}", GLType (p.Type), p.Name))
				.SeparateWith (", ");
			CodeOut ("{0} {1} ({2})", GLType (expr.ReturnType), name, pars);
			CodeOut ("{");
			_tabLevel++;
			FunctionBody (expr.Body);
			EndFunction ();
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
				_mainStart = _code.Length;
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
					return null;
                }) ??
				expr.Match<InvocationExpression, string> (ie => 
				{
					var	member = ie.Expression.Expect<MemberExpression> (ExpressionType.MemberAccess).Member;
					if (_functions.ContainsKey (member))
					{
						_funcRefs.Add (member);
						return string.Format ("{0} ({1})", member.Name,
							ie.Arguments.Select (a => ExprToGLSL (a)).SeparateWith (", "));
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

		private MethodCallExpression CastDeclaration (Expression expr)
		{
			var me = expr.CastExpr<MethodCallExpression> (ExpressionType.Call);
			return  me != null && me.Method.IsDeclaration () ? me : null;
		}

        public bool Declaration (Source source)
        {
			var arg1 = CastDeclaration (source.Current.GetSelectLambda ().Body);
			if (arg1 == null)
				return false;
			var arg0 = CastDeclaration (source.Current.Arguments[0]);
			if (arg0 != null)
				Declarations (arg0);
			Declarations (arg1);
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
            var aggrFun = node.Arguments[2].Expect<LambdaExpression> (ExpressionType.Lambda);
            var accum = aggrFun.Parameters[0];
            var iterVar = aggrFun.Parameters[1];
            CodeOut ("{0} {1} = {2};", GLType(accum.Type), accum.Name, 
                ExprToGLSL (node.Arguments[1]));
            var se = node.Arguments[0].ExpectSelect ();
            ForParser ().Execute (new Source (se.Arguments[0].Traverse ()));
            CodeOut ("{0} {1} = {2};", GLType (iterVar.Type), iterVar.Name, 
                ExprToGLSL (se.Arguments[1].ExpectLambda ().Body));
            CodeOut ("{0} = {1};", accum.Name, ExprToGLSL (aggrFun.Body));
            _tabLevel--;
            CodeOut ("}");
            return accum.Name;
        }

        public bool ForLoop (Source source)
        {
			var array = source.Current.Arguments[0];
            var field = array.SkipUnary (ExpressionType.Not)
                .Expect<MemberExpression> (ExpressionType.MemberAccess).Member as FieldInfo;
            if (field == null)
				throw new ParseException ("Invalid array expression. " +
					"Expected uniform field reference. Encountered: " + array);
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

		public void OutputShader (LambdaExpression expr)
        {
			var mce = expr.Body.ExpectSelect ();
			var me = CastDeclaration (mce.Arguments[0]);
			if (me != null)
            {
				Declarations (me);
                StartMain ();
            } else
                ShaderParser ().Execute (new Source (mce.Arguments[0].Traverse ()));
            Return (mce.Arguments[1].ExpectLambda ().Body);
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
			var mce = expr.GetSelect ();
			if (mce != null)
			{
				var ne = mce.Arguments [0].CastExpr<NewExpression> (ExpressionType.New);
				if (ne == null)
					Parse.ZeroOrMore (LetBinding).Execute (new Source (mce.Arguments [0].Traverse ()));
				CodeOut ("return {0};", ExprToGLSL (mce.Arguments[1].ExpectLambda ().Body));
			}
			else
				CodeOut ("return {0};", ExprToGLSL (expr));
		}
    }
}
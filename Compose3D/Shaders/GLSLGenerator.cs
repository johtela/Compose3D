namespace Compose3D.Shaders
{
    using Compose3D;
	using GLTypes;
    using OpenTK.Graphics.OpenGL;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
	using Extensions;

	public class GLSLGenerator
    {
		internal static Dictionary<MemberInfo, Function> _functions = new Dictionary<MemberInfo, Function> ();
		private StringBuilder _decl;
		private StringBuilder _code;
		private HashSet<Type> _typesDefined;
		private HashSet<Function> _funcRefs;
		private Dictionary<string, Constant> _constants;
        private int _localVarCount;
        private int _tabLevel;

		private GLSLGenerator ()
        {
			_decl = new StringBuilder ();
			_code = new StringBuilder ();
            _typesDefined = new HashSet<Type> ();
			_funcRefs = new HashSet<Function> ();
			_constants = new Dictionary<string, Constant> ();
        }

		public static string CreateShader<T> (Expression<Func<Shader<T>>> shader)
        {
			var builder = new GLSLGenerator ();
            var glslVersion = GL.GetString(StringName.ShadingLanguageVersion);
            var match = new Regex (@"(\d+)\.([^\-]+).*").Match (glslVersion);
            if (!match.Success || match.Groups.Count != 3)
                throw new ParseException ("Invalid GLSL version string: " + glslVersion);
            var version = match.Groups[1].Value + match.Groups[2].Value;
            builder.DeclareVariables (typeof (T), "out");
            builder.OutputShader (shader);
            return string.Format ("#version {0}\nprecision highp float;\n", version) +
				builder._decl.ToString() +
				GenerateFunctions (builder._funcRefs) +
				builder._code.ToString ();
        }

		public static void CreateFunction (MemberInfo member, LambdaExpression expr)
		{
			var builder = new GLSLGenerator ();
			builder.OutputFunction (member.Name, expr);
			_functions.Add (member, new Function (member, builder._code.ToString (), builder._funcRefs));
		}

		private static string GenerateFunctions (HashSet<Function> functions)
		{
			if (functions.Count == 0)
				return "";
			var outputted = new HashSet<Function> ();
			var sb = new StringBuilder ();
			sb.AppendLine ();
			foreach (var fun in functions)
				fun.Output (sb, outputted);
			return sb.ToString ();
		}
        
        private string Tabs ()
        {
            var sb = new StringBuilder ();
            for (int i = 0; i < _tabLevel; i++)
                sb.Append ("    ");
            return sb.ToString ();
        }

		private void DeclOut (string code, params object[] args)
		{
			if (args.Length == 0)
				_decl.AppendLine (code);
			else
				_decl.AppendFormat (code + "\n", args);
		}

        private void CodeOut (string code, params object[] args)
		{
			if (args.Length == 0)
				_code.AppendLine (Tabs () + code);
			else
				_code.AppendFormat (Tabs () + code + "\n", args);
		}

		private bool DefineType (Type type)
		{
			if (_typesDefined.Contains (type))
				return false;
			_typesDefined.Add (type);
			return true;
		}

        private void OutputStruct (Type structType)
		{
			if (!DefineType (structType)) return;
			foreach (var field in structType.GetGLFields ())
				if (field.FieldType.IsGLStruct ())
					OutputStruct (field.FieldType);
			DeclOut ("struct {0}\n{{", structType.Name);
			foreach (var field in structType.GetGLFields ())
				DeclareVariable (field, field.FieldType, "    ");
			DeclOut ("};");
		}

        private void DeclareUniforms (Type type)
        {
			if (!DefineType (type)) return;
			foreach (var field in type.GetUniformsRecursively ())
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
				DeclOut ("uniform {0} {1}{2};", glAttr.Syntax, field.Name, 
                    arrayLen > 0 ? "[" + arrayLen + "]" : "");
            }
        }

        private void DeclareVariable (MemberInfo member, Type memberType, string prefix)
        {
            var syntax = GLType (memberType); 
            if (!(member.IsBuiltin () || member.IsDefined (typeof (OmitInGlslAttribute), true) ||
				member.Name.StartsWith ("<>")))
            {
                var qualifiers = member.GetQualifiers ();
				DeclOut (string.IsNullOrEmpty (qualifiers) ?
                    string.Format ("{0} {1} {2};", prefix, syntax, member.Name) :
                    string.Format ("{0} {1} {2} {3};", qualifiers, prefix, syntax, member.Name));
            }
        }

        private void DeclareVariables (Type type, string prefix)
        {
			if (!DefineType (type))
				return;
            if (!type.Name.StartsWith ("<>"))
				foreach (var field in type.GetGLFields ())
					DeclareVariable (field, field.FieldType, prefix);
			foreach (var prop in type.GetGLProperties ())
				DeclareVariable (prop, prop.PropertyType, prefix);
		}

		private void DeclareConstants (Expression expr)
		{
			var ne = expr.CastExpr<NewExpression> (ExpressionType.New);
			if (ne == null)
			{
				var mie = expr.CastExpr<MemberInitExpression> (ExpressionType.MemberInit);
				if (mie == null)
					throw new ParseException ("Unsupported shader expression: " + expr);
				foreach (MemberAssignment assign in mie.Bindings)
				{
					var memberType = assign.Member is FieldInfo ?
						(assign.Member as FieldInfo).FieldType :
						(assign.Member as PropertyInfo).PropertyType;
					OutputConst (memberType, assign.Member.Name, assign.Expression);
				}
			}
			else
			{
				for (int i = 0; i < ne.Members.Count; i++)
				{
					var prop = (PropertyInfo)ne.Members[i];
					if (!prop.Name.StartsWith ("<>"))
						OutputConst (prop.PropertyType, prop.Name, ne.Arguments[i]);
				}
			}
		}

		private void OutputConst (Type constType, string name, Expression value)
		{
			_constants.Add (name, new Constant (constType, name, value));
			if (constType.IsArray)
			{
				var elemType = constType.GetElementType ();
				var elemGLType = GLType (elemType);
				var nai = value.Expect<NewArrayExpression> (ExpressionType.NewArrayInit);
				CodeOut ("const {0} {1}[{2}] = {0}[] ( {3} );",
					elemGLType, name, nai.Expressions.Count,
					nai.Expressions.Select (ExprToGLSL).SeparateWith (",\n\t"));
			}
			else
				CodeOut ("const {0} {1} = {2};", GLType (constType), name, ExprToGLSL (value));
		}

		private void OutputFromBinding (ParameterExpression par, MethodCallExpression node)
		{
			var type = node.Method.GetGenericArguments () [0];
			if (node.Method.Name == "Inputs")
				DeclareVariables (type, "in");
			else if (node.Method.Name == "Uniforms")
				DeclareUniforms (type);
			else if (node.Method.Name == "Constants")
				DeclareConstants (node.Arguments [0]);
			else if (node.Method.Name == "ToShader")
				CodeOut ("{0} {1} = {2};", GLType (type), par.Name, ExprToGLSL (node.Arguments [0]));
			else
				throw new ArgumentException ("Unsupported lift method.", node.Method.ToString ());
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
            CodeOut ("void main ()");
            CodeOut ("{");
            _tabLevel++;
        }

        private string GLType (Type type)
        {
            var attr = type.GetGLAttribute ();
            if (attr == null) 
				return TypeMapping.Type (type);
			if (attr is GLStruct)
				OutputStruct (type);
			return attr.Syntax;
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
                        attr != null ? attr.Syntax : TypeMapping.Operator (be.NodeType),
                        ExprToGLSL (be.Left), ExprToGLSL (be.Right)) + ")";

                }) ??
                expr.Match<UnaryExpression, string> (ue =>
                {
                    var attr = ue.Method.GetGLAttribute ();
                    return string.Format (attr != null ? attr.Syntax :
 						ue.NodeType == ExpressionType.Convert ?
							string.Format ("{0} ({1})", TypeMapping.Type (ue.Type), ExprToGLSL (ue.Operand)) :
							TypeMapping.Operator (ue.NodeType), ExprToGLSL (ue.Operand));
                }) ??
                expr.Match<MethodCallExpression, string> (mc =>
                {
                    var attr = mc.Method.GetGLAttribute ();
					if (attr == null && mc.Method.Name == "get_Item")
						return string.Format ("{0}.{1}", ExprToGLSL (mc.Object),
							mc.Arguments.Select (a => ExprToGLSL (a)).SeparateWith (""));
					var args = mc.Method.IsStatic ? mc.Arguments : mc.Arguments.Prepend (mc.Object);
					return string.Format (attr != null ? attr.Syntax : TypeMapping.Function (mc.Method),
						args.Select (a => ExprToGLSL (a)).SeparateWith (", "));
                }) ??
				expr.Match<InvocationExpression, string> (ie => 
				{
					var	member = ie.Expression.Expect<MemberExpression> (ExpressionType.MemberAccess).Member;
					Function fun;
					if (_functions.TryGetValue (member, out fun))
					{
						_funcRefs.Add (fun);
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
                        me.Expression.Type.IsGLType () ?
                            string.Format ("{0}.{1}", ExprToGLSL (me.Expression), me.Member.GetGLFieldName ()) :
                            me.Member.Name;
                }) ??
                expr.Match<NewExpression, string> (ne =>
                {
                    var attr = ne.Constructor.GetGLAttribute ();
                    return attr == null ? null :
                        string.Format (attr.Syntax, ne.Arguments.Select (a => ExprToGLSL (a)).SeparateWith (", "));
                }) ??
                expr.Match<ConstantExpression, string> (ce => ce.Type == typeof(float) ? 
						string.Format (CultureInfo.InvariantCulture, "{0:0.0############}f", ce.Value) :
						string.Format (CultureInfo.InvariantCulture, "{0}", ce.Value)
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

		private MethodCallExpression CastFromBinding (Expression expr)
		{
			var me = expr.CastExpr<MethodCallExpression> (ExpressionType.Call);
			return  me != null && me.Method.IsLiftMethod () ? me : null;
		}

        public bool FromBinding (Source source)
        {
			var mce = source.Current;
			var arg0 = CastFromBinding (mce.Arguments[0]);
			if (mce.Method.IsSelectMany ())
			{
				var arg1 = CastFromBinding (mce.GetSelectLambda ().Body);
				if (arg1 == null)
					return false;
				if (arg0 != null)
					OutputFromBinding (mce.Arguments [1].GetLambdaParameter (0), arg0);
				OutputFromBinding (mce.Arguments [2].GetLambdaParameter (1), arg1);
			}
			else
			{
				if (arg0 == null)
					return false;
				var le = mce.Arguments [1].ExpectLambda ();
				OutputFromBinding (le.Parameters[0], arg0);
				OutputLet (le.Body.Expect<NewExpression> (ExpressionType.New));
			}
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
			var se = node.Arguments[0].GetSelect ();
			if (se != null)
			{
				ParseFor (se);
				CodeOut ("{0} {1} = {2};", GLType (iterVar.Type), iterVar.Name,
					ExprToGLSL (se.Arguments[1].ExpectLambda ().Body));
			}
			else
				OutputForLoop (node);
            CodeOut ("{0} = {1};", accum.Name, ExprToGLSL (aggrFun.Body));
            _tabLevel--;
            CodeOut ("}");
            return accum.Name;
        }

		public void ParseFor (MethodCallExpression mce)
		{
			if (mce.Arguments[0].GetSelect () == null)
				OutputForLoop (mce);
			else
				Parse.ExactlyOne (ForLoop).IfFail (new ParseException (
					"Must have exactly one from clause in the beginning of aggregate expression."))
					.Then (Parse.ZeroOrMore (LetBinding))
					.Execute (new Source (mce.Arguments[0].Traverse ()));
		}

		public void OutputForLoop (MethodCallExpression expr)
        {
			var array = expr.Arguments[0];
            var member  = array.SkipUnary (ExpressionType.Not)
                .Expect<MemberExpression> (ExpressionType.MemberAccess).Member;
			var len = 0;
			var field = member as FieldInfo;
			if (field != null)
				len = field.ExpectGLArrayAttribute ().Length;
			else if (_constants.ContainsKey (member.Name))
			{
				var constant = _constants[member.Name];
				if (!constant.Type.IsArray)
					throw new ParseException ("Invalid array expression. Referenced constant is not an array.");
				var nai = constant.Value.Expect<NewArrayExpression> (ExpressionType.NewArrayInit);
				len = nai.Expressions.Count;
			}
			else
				throw new ParseException ("Invalid array expression. " +
					"Expected uniform field reference or constant array. Encountered: " + array);
			var indexVar = NewLocalVar ("ind");
			var item = expr.Method.IsSelect () ?
				expr.GetSelectLambda ().Parameters[0] :
				expr.Arguments[2].ExpectLambda ().Parameters[1];
            CodeOut ("for (int {0} = 0; {0} < {1}; {0}++)", indexVar, len);
            CodeOut ("{");
            _tabLevel++;
            CodeOut ("{0} {1} = {2}[{3}];", GLType (item.Type), item.Name, 
                ExprToGLSL (array), indexVar);
        }

		public bool ForLoop (Source source)
		{
			var se = source.Current;
			OutputForLoop (se);
			OutputLet (se.GetSelectLambda ().Body.Expect<NewExpression> (ExpressionType.New));
			return true;
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
			StartMain ();
			var retExpr = ParseShader (expr.Body);
            Return (retExpr);
			EndFunction ();
        }

		Expression ParseShader (Expression expr)
		{
			var mce = expr.ExpectSelect ();
			var me = CastFromBinding (mce.Arguments [0]);
			if (me != null)
				OutputFromBinding (mce.Arguments [1].GetLambdaParameter (0), me);
			else
				Parse.OneOrMore (FromBinding).Then (Parse.ZeroOrMore (LetBinding))
					.Execute (new Source (mce.Arguments [0].Traverse ()));
			me = CastFromBinding (mce.Arguments [1].ExpectLambda ().Body);
			if (me != null)
			{
				OutputFromBinding (mce.Arguments [2].GetLambdaParameter (1), me);
				return mce.Arguments [2].ExpectLambda ().Body;
			}
			return mce.Arguments[1].ExpectLambda ().Body;
		}

		public void FunctionBody (Expression expr)
		{
			var node = expr.CastExpr<MethodCallExpression> (ExpressionType.Call);
			CodeOut ("return {0};", ExprToGLSL (node != null && node.Method.IsEvaluate () ?
				ParseShader (node.Arguments [0]) : expr));
		}
	}
}
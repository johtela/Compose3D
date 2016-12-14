namespace Compose3D.Compiler
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text;
	using Extensions;

	public abstract class LinqCompiler
    {
		internal static Dictionary<MemberInfo, Function> _functions = new Dictionary<MemberInfo, Function> ();
		private StringBuilder _decl;
		private StringBuilder _code;
		private HashSet<Type> _typesDefined;
		private HashSet<Function> _funcRefs;
		private Dictionary<string, Constant> _constants;
        private int _indexVarCount;
        private int _tabLevel;
		private Type _linqType;
		private TypeMapping _typeMapping;

		protected LinqCompiler (Type linqType, TypeMapping typeMapping)
        {
			_decl = new StringBuilder ();
			_code = new StringBuilder ();
            _typesDefined = new HashSet<Type> ();
			_funcRefs = new HashSet<Function> ();
			_constants = new Dictionary<string, Constant> ();
			_linqType = linqType;
			_typeMapping = typeMapping;
        }

		protected virtual string MapType (Type type)
		{
			var result = _typeMapping.Type (type);
			if (result == null)
				throw new ParseException ("No mapping defined for type: " + type.Name);
			return result;
		}

		protected string MapFunction (MethodInfo method)
		{
			var result = _typeMapping.Function (method);
			if (result == null)
				throw new ParseException ("No mapping defined for method: " + method.Name);
			return result;
		}

		protected string MapOperator (MethodInfo method, ExpressionType expression)
		{
			var result = _typeMapping.Operator (method, expression);
			if (result == null)
				throw new ParseException ("No mapping defined for operator: " + expression);
			return result;
		}

		protected string MapConstructor (ConstructorInfo constructor)
		{
			var result = _typeMapping.Constructor (constructor);
			if (result == null)
				throw new ParseException ("No mapping defined for constructor: " + constructor.Name);
			return result;
		}

		protected abstract string MapMemberAccess (MemberExpression member);

		protected abstract void OutputFromBinding (ParameterExpression par, MethodCallExpression node);

		public static void CreateFunction (LinqCompiler compiler, MemberInfo member, LambdaExpression expr)
		{
			compiler.OutputFunction (member.Name, expr);
			_functions.Add (member, new Function (member, compiler._code.ToString (), compiler._funcRefs));
		}

		internal static string GenerateFunctions (HashSet<Function> functions)
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

		protected void DeclOut (string code, params object[] args)
		{
			if (args.Length == 0)
				_decl.AppendLine (code);
			else
				_decl.AppendFormat (code + "\n", args);
		}

		protected void CodeOut (string code, params object[] args)
		{
			if (args.Length == 0)
				_code.AppendLine (Tabs () + code);
			else
				_code.AppendFormat (Tabs () + code + "\n", args);
		}

		protected bool DefineType (Type type)
		{
			if (_typesDefined.Contains (type))
				return false;
			_typesDefined.Add (type);
			return true;
		}

		protected void OutputFunction (string name, LambdaExpression expr)
		{
			var pars = (from p in expr.Parameters
			            select string.Format ("{0} {1}", MapType (p.Type), p.Name))
				.SeparateWith (", ");
			CodeOut ("{0} {1} ({2})", MapType (expr.ReturnType), name, pars);
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

        private string NewIndexVar (string name)
        {
            return string.Format ("_gen_{0}{1}", name, ++_indexVarCount);
        }

        private string Expr (Expression expr)
        {
            var result =
                expr.Match<BinaryExpression, string> (be =>
					"(" + string.Format (MapOperator (be.Method, be.NodeType), 
					Expr (be.Left), Expr (be.Right)) + ")") 
				??
                expr.Match<UnaryExpression, string> (ue =>
                    string.Format (ue.NodeType == ExpressionType.Convert ?
						string.Format ("{0} ({1})", MapType (ue.Type), Expr (ue.Operand)) :
						MapOperator (ue.Method, ue.NodeType), Expr (ue.Operand))) 
				??
                expr.Match<MethodCallExpression, string> (mc =>
                {
					if (mc.Method.Name == "get_Item")
						return string.Format ("{0}.{1}", Expr (mc.Object),
							mc.Arguments.Select (a => Expr (a)).SeparateWith (""));
					var args = mc.Method.IsStatic ? mc.Arguments : mc.Arguments.Prepend (mc.Object);
					return string.Format (MapFunction (mc.Method),
						args.Select (a => Expr (a)).SeparateWith (", "));
                }) ??
				expr.Match<InvocationExpression, string> (ie => 
				{
					var	member = ie.Expression.Expect<MemberExpression> (ExpressionType.MemberAccess).Member;
					Function fun;
					if (_functions.TryGetValue (member, out fun))
					{
						_funcRefs.Add (fun);
						return string.Format ("{0} ({1})", member.Name,
							ie.Arguments.Select (a => Expr (a)).SeparateWith (", "));
					}
					throw new ParseException ("Undefined function: " + member.Name);
				}) ??
                expr.Match<MemberExpression, string> (MapMemberAccess)
                ??
                expr.Match<NewExpression, string> (ne =>
					string.Format (MapConstructor (ne.Constructor), ne.Arguments.Select (a => 
						Expr (a)).SeparateWith (", ")))
                ??
                expr.Match<ConstantExpression, string> (ce => ce.Type == typeof(float) ? 
					string.Format (CultureInfo.InvariantCulture, "{0:0.0############}f", ce.Value) :
					string.Format (CultureInfo.InvariantCulture, "{0}", ce.Value)) 
				?? 
				expr.Match<NewArrayExpression, string> (na => string.Format ("{0}[{1}] (\n\t{2})",
					MapType (na.Type.GetElementType ()), na.Expressions.Count,
					na.Expressions.Select (Expr).SeparateWith (",\n\t"))) 
				??
                expr.Match<ConditionalExpression, string> (ce => string.Format ("({0} ? {1} : {2})", 
                    Expr (ce.Test), Expr (ce.IfTrue), Expr (ce.IfFalse))) 
				?? 
                expr.Match<ParameterExpression, string> (pe => pe.Name) 
				?? 
				null;
            if (result == null)
                throw new ParseException (string.Format ("Unsupported expression type {0}", expr));
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
			if (mce.Method.IsSelectMany (_linqType))
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
				if (!(prop.Name.StartsWith ("<>") || ne.Arguments[i] is ParameterExpression))
				{
					var type = MapType (prop.PropertyType);
					var val = Expr (RemoveAggregates (ne.Arguments[i]));
					if (prop.Name != val)
						CodeOut ("{0} {1} = {2};", type, prop.Name, val);
				}
            }
            return true;
        }

		public Expression RemoveAggregates (Expression expr)
		{
			return expr.ReplaceSubExpression<MethodCallExpression> (ExpressionType.Call, Aggregate);
		}

        public Expression Aggregate (MethodCallExpression expr)
        {
            if (!expr.Method.IsAggregate ())
                return expr;
            var aggrFun = expr.Arguments[2].Expect<LambdaExpression> (ExpressionType.Lambda);
            var accum = aggrFun.Parameters[0];
            var iterVar = aggrFun.Parameters[1];
            CodeOut ("{0} {1} = {2};", MapType (accum.Type), accum.Name, 
                Expr (expr.Arguments[1]));
			var se = expr.Arguments[0].GetSelect (_linqType);
			if (se != null)
			{
				ParseFor (se);
				CodeOut ("{0} {1} = {2};", MapType (iterVar.Type), iterVar.Name,
					Expr (se.Arguments[1].ExpectLambda ().Body));
			}
			else
			{
				var mce = expr.Arguments [0].CastExpr<MethodCallExpression> (ExpressionType.Call);
				if (mce != null && mce.Method.IsRange ())
					OutputForLoop (expr);
				else
					IterateArray (expr);
			}
            CodeOut ("{0} = {1};", accum.Name, Expr (aggrFun.Body));
            _tabLevel--;
            CodeOut ("}");
            return accum;
        }

		public void ParseFor (MethodCallExpression mce)
		{
			if (mce.Arguments[0].GetSelect (_linqType) == null)
				IterateArray (mce);
			else
				Parse.ExactlyOne (ForLoop).IfFail (new ParseException (
					"Must have exactly one from clause in the beginning of aggregate expression."))
					.Then (Parse.ZeroOrMore (LetBinding))
					.Execute (new Source (mce.Arguments[0].Traverse (_linqType)));
		}

		public void IterateArray (MethodCallExpression expr)
        {
			var array = expr.Arguments[0];
            var member  = array.SkipUnary (ExpressionType.Not)
                .Expect<MemberExpression> (ExpressionType.MemberAccess).Member;
			var len = 0;
			var field = member as FieldInfo;
			if (field != null)
				len = field.ExpectFixedArrayAttribute ().Length;
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
			var indexVar = NewIndexVar ("ind");
			var item = expr.Method.IsSelect (_linqType) ?
				expr.GetSelectLambda ().Parameters[0] :
				expr.Arguments[2].ExpectLambda ().Parameters[1];
            CodeOut ("for (int {0} = 0; {0} < {1}; {0}++)", indexVar, len);
            CodeOut ("{");
            _tabLevel++;
            CodeOut ("{0} {1} = {2}[{3}];", MapType (item.Type), item.Name, 
                Expr (array), indexVar);
        }

		public void OutputForLoop (MethodCallExpression expr)
		{
			var indexVar = expr.Method.IsSelect (_linqType) ?
				expr.GetSelectLambda ().Parameters[0] :
				expr.Arguments[2].ExpectLambda ().Parameters[1];
			var range = expr.Arguments[0].Expect<MethodCallExpression> (ExpressionType.Call);
			var start = Expr (range.Arguments[0]);
			if (range.Method.DeclaringType == typeof (Enumerable))
			{
				var len = Expr (range.Arguments[1]);
				CodeOut ("for (int {0} = {1}; {0} < {2}; {0}++)", indexVar, start, len);
			}
			else
			{
				var end = Expr (range.Arguments[1]);
				var step = Expr (range.Arguments[2]);
				CodeOut ("for (int {0} = {1}; {0} != {2}; {0} += {3})", indexVar, start, end, step);
			}
			CodeOut ("{");
			_tabLevel++;
		}

		public bool ForLoop (Source source)
		{
			var se = source.Current;
			IterateArray (se);
			OutputLet (se.GetSelectLambda ().Body.Expect<NewExpression> (ExpressionType.New));
			return true;
		}

		public bool Where (Source source)
		{
			if (!source.Current.Method.IsWhere (_linqType))
				return false;
			var predicate = source.Current.Arguments[1].ExpectLambda ().Body;
			CodeOut ("if (!{0}) return;", Expr (predicate));
			return true;
		}

        public void Return (Expression expr)
        {
			expr = RemoveAggregates (expr);
            var ne = expr.CastExpr<NewExpression> (ExpressionType.New);
            if (ne == null)
            {
                var mie = expr.CastExpr<MemberInitExpression> (ExpressionType.MemberInit);
                if (mie == null)
                    throw new ParseException ("Unsupported expression: " + expr);
                foreach (MemberAssignment assign in mie.Bindings)
                    CodeOut ("{0} = {1};", assign.Member.Name, Expr (assign.Expression));
            }
            else
            {
                for (int i = 0; i < ne.Members.Count; i++)
                {
                    var prop = (PropertyInfo)ne.Members[i];
                    if (!prop.Name.StartsWith ("<>"))
                        CodeOut ("{0} = {1};", prop.Name, Expr (ne.Arguments[i]));
                }
            }
        }

		public void ConditionalReturn (Expression expr, Action<Expression> returnAction)
		{
			var ce = expr.CastExpr<ConditionalExpression> (ExpressionType.Conditional);
			if (ce == null)
				returnAction (expr);
			else
			{
				CodeOut ("if ({0})", Expr (ce.Test));
				CodeOut ("{");
				_tabLevel++;
				returnAction (ce.IfTrue);
				_tabLevel--;
				CodeOut ("}");
				CodeOut ("else");
				CodeOut ("{");
				_tabLevel++;
				ConditionalReturn (ce.IfFalse, returnAction);
				_tabLevel--;
				CodeOut ("}");
			}
		}

		Expression ParseLinqExpression (Expression expr)
		{
			var mce = expr.ExpectSelect (_linqType);
			var me = CastFromBinding (mce.Arguments [0]);
			if (me != null)
				OutputFromBinding (mce.Arguments [1].GetLambdaParameter (0), me);
			else
				Parse.OneOrMore (FromBinding).Then (Parse.ZeroOrMore (Parse.Either (LetBinding, Where)))
					.Execute (new Source (mce.Arguments [0].Traverse (_linqType)));
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
			CodeOut ("return {0};", Expr (RemoveAggregates (
				node != null && node.Method.IsEvaluate (_linqType) ? ParseLinqExpression (node.Arguments [0]) : expr)));
		}
	} 
}
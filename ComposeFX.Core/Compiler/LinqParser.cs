namespace ComposeFX.Compiler
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
	using Extensions;

	public abstract class LinqParser
    {
		protected HashSet<Type> _typesDefined;
		protected Ast.Program _program;
		protected Ast.Function _function;
		protected Scope _currentScope;
		protected Dictionary<string, Ast.Constant> _constants;
		protected Dictionary<string, Ast.Variable> _globalVars;
		protected Dictionary<string, Ast.Global> _globals;
		protected Type _linqType;
		protected TypeMapping _typeMapping;

		protected LinqParser (Type linqType, TypeMapping typeMapping)
        {
            _typesDefined = new HashSet<Type> ();
			_constants = new Dictionary<string, Ast.Constant> ();
			_globalVars = new Dictionary<string, Ast.Variable> ();
			_globals = new Dictionary<string, Ast.Global> ();
			_linqType = linqType;
			_typeMapping = typeMapping;
			_program = Ast.Prog ();
        }

		protected internal virtual string MapType (Type type)
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

		protected virtual Ast.Expression MapMemberAccess (MemberExpression me)
		{
			Ast.Variable v = _currentScope.FindLocalVar (me.Member.Name);
			if (v != null)
				return Ast.VRef (v);
			if (_globalVars.TryGetValue (me.Member.Name, out v))
				return Ast.VRef (v);
            if (_constants.TryGetValue (me.Member.Name, out Ast.Constant c))
                return Ast.VRef (c);
            throw new ParseException ("Access to undefined member " + me.Member.Name);
		}

		protected abstract string MapTypeCast (Type type);

		protected abstract void OutputFromBinding (ParameterExpression par, MethodCallExpression node);

		protected abstract Ast.NewArray ArrayConstant (Type type, int count, IEnumerable<Ast.Expression> items);

		protected abstract Ast.InitStruct InitStructure (Type type, 
			IEnumerable<Tuple<Ast.VariableRef, Ast.Expression>> initFields);

		protected static void CreateFunction (LinqParser parser, MemberInfo member, LambdaExpression expr)
		{
			parser.Function (ConstructFunctionName (member), expr);
			parser._program.Functions.Add (Macro.InstantiateAllMacros (parser._function));
			Compiler.Function.Add (member, new Function (parser._program, parser._function, 
				parser._typesDefined));
		}

		protected static void CreateMacro (LinqParser parser, MemberInfo member, LambdaExpression expr)
		{
			var macro = parser.ParseMacro (expr);
			Macro.Add (member, new Macro (macro, parser._program, parser._typesDefined));
		}

		protected Ast.Macro ParseMacro (LambdaExpression expr)
		{
			var def = Macro.GetMacroDefinition (expr.Type);
			var parameters = expr.Parameters.Zip (def.Parameters, (p, mp) =>
				new KeyValuePair<ParameterExpression, Ast.MacroParam> (p, mp));
			var block = Ast.Blk ();
			_currentScope = MacroScope.Begin (_currentScope, block, parameters);
			MacroBody (def, expr.Body);
			EndScope ();
			return Ast.Mac (def.Parameters, def.Result, block);
		}

		protected virtual IEnumerable<Ast.Argument> FunctionArguments (IEnumerable<ParameterExpression> pars)
		{
			return pars.Select (p =>
			{
				var arg = Ast.Arg (p.Type, p.Name);
				_currentScope.AddLocal (arg.Name, arg);
				return arg;
			});
		}

		protected static string ConstructFunctionName (MemberInfo member)
		{
			return member.DeclaringType.Name + "_" + member.Name;
		}

		protected void Function (string name, LambdaExpression expr)
		{
			var block = Ast.Blk ();
			BeginScope (block);
			var args = FunctionArguments (expr.Parameters).ToArray ();
			_function = Ast.Fun (name, expr.ReturnType, args, block);
			FunctionBody (expr.Body);
			EndScope ();
		}

		protected void BeginScope (Ast.Block block)
		{
			_currentScope = Scope.Begin (_currentScope, block);
		}

		protected void EndScope ()
		{
			_currentScope = _currentScope.End ();
		}

		protected void AddGlobal (Ast.Global global)
		{
			_program.Globals.Add (global);
			_globals.Add (global.Name, global);
		}

		protected void DeclOut (string declaration, params object[] args)
		{
			AddGlobal (Ast.Decl (args.Length == 0 ?
				declaration :
				string.Format (declaration, args)));
		}

		protected bool DefineType (Type type)
		{
			if (_typesDefined.Contains (type))
				return false;
			_typesDefined.Add (type);
			return true;
		}

		protected string StructTypeName (Type structType)
		{
			var name = structType.Name;
			return structType.IsConstructedGenericType ?
				string.Format ("{0}_{1}",
					name.Substring (0, name.IndexOf ('`')),
					structType.GetGenericArguments ().Select (MapType).SeparateWith ("_")) :
				name;
		}

        protected Ast.Expression Expr (Expression expr)
        {
            return ChooseExpr ((dynamic)expr);
        }

        protected Ast.Expression ChooseExpr (BinaryExpression be)
        {
            return Ast.Op (MapOperator (be.Method, be.NodeType), 
                Expr (be.Left), Expr (be.Right));
        }

        protected Ast.Expression ChooseExpr (UnaryExpression ue)
        {
            return Ast.Op (
                ue.NodeType == ExpressionType.Convert ?
                    MapTypeCast (ue.Type) :
                    MapOperator (ue.Method, ue.NodeType),
                Expr (ue.Operand));
        }

        protected Ast.Expression ChooseExpr (MethodCallExpression mc)
        {
            if (mc.Method.Name == "get_Item")
            {
                if (mc.Arguments[0].Type == typeof (Maths.Coord))
                    return Ast.FRef (Expr (mc.Object),
                        Ast.Fld (mc.Arguments.Select (a =>
                            Expr (a).Output (this)).SeparateWith ("")));
                var syntax = _typeMapping.Indexer (mc.Method);
                return syntax == null || mc.Arguments.Count != 1 ?
                    null :
                    Ast.Op (syntax, Expr (mc.Object), Expr (mc.Arguments[0]));
            }
            var args = mc.Method.IsStatic ? mc.Arguments : mc.Arguments.Prepend (mc.Object);
            return Ast.Call (MapFunction (mc.Method), args.Select (Expr));
        }

        protected Ast.Expression ChooseExpr (InvocationExpression ie)
        {
            var me = ie.Expression.CastExpr<MemberExpression> (ExpressionType.MemberAccess);
            if (me != null)
                return FunctionCall (ie, me.Member);
            throw new ParseException ("Function call is allowed only for static members.");
        }

        protected Ast.Expression ChooseExpr (MemberExpression me)
        {
            return MapMemberAccess (me);
        }

        protected Ast.Expression ChooseExpr (NewExpression ne)
        {
            return ne.Constructor == null ? null :
                Ast.Call (MapConstructor (ne.Constructor), ne.Arguments.Select (Expr));
        }

        protected Ast.Expression ChooseExpr (ConstantExpression ce)
        {
            return Ast.Lit (string.Format (CultureInfo.InvariantCulture,
                ce.Type == typeof (float) ? "{0:0.0############}f" : "{0}", ce.Value)); 
        }

        protected Ast.Expression ChooseExpr (NewArrayExpression na)
        {
            return ArrayConstant (na.Type.GetElementType (), na.Expressions.Count,
                na.Expressions.Select (Expr));
        }

        protected Ast.Expression ChooseExpr (ConditionalExpression ce)
        {
            return Ast.Cond (Expr (ce.Test), Expr (ce.IfTrue),
                Expr (ce.IfFalse));
        }

        protected Ast.Expression ChooseExpr (ParameterExpression pe)
        {
            var local = _currentScope.FindLocalVar (pe.Name);
            if (local != null)
                return Ast.VRef (local);
            var mpar = _currentScope.FindMacroParam (pe);
            if (mpar != null)
                return Ast.MPRef (mpar);
            throw new ParseException ("Reference to undefined local variable: " + pe.Name);
        }

        protected Ast.Expression ChooseExpr (MemberInitExpression mi)
        {
            MapType (mi.Type);
            var astruct = (Ast.Structure)_globals[StructTypeName (mi.Type)];
            var fieldInits = from b in mi.Bindings.Cast<MemberAssignment> ()
                             let field = astruct.Fields.Find (f => f.Name == b.Member.Name)
                             select Tuple.Create (Ast.VRef (field), Expr(b.Expression));
            return InitStructure (mi.Type, fieldInits);
        }

        protected Ast.Expression ChooseExpr (Expression expr)
        {
            throw new ParseException (string.Format ("Unsupported expression type {0}", expr));
        }

        private void AddExternalReferences (Ast.Program program, HashSet<Type> typesDefined)
		{
			if (typesDefined == null || program == null)
				return;
			_typesDefined.UnionWith (typesDefined);
			foreach (var glob in program.Globals)
				if (!_globals.ContainsKey (glob.Name))
					AddGlobal (glob);
			foreach (var func in program.Functions)
				if (!_program.Functions.Contains (func))
					_program.Functions.Add (func);
		}

		private Ast.FunctionCall FunctionCall (InvocationExpression ie, MemberInfo member)
		{
			InitializeFunction (member);
			var fun = Compiler.Function.Get (member);
			if (fun != null)
			{
				if (!_program.Functions.Contains (fun.AstFunction))
					AddExternalReferences (fun.Program, fun.TypesDefined);
				return Ast.Call (fun.AstFunction, ie.Arguments.Select (Expr));
			}
			throw new ParseException ("Undefined function: " + ConstructFunctionName (member));
		}

		private static void InitializeFunction (MemberInfo member)
		{
			if (!Compiler.Function.IsDefined (member))
				InitializeStaticMember (member);
		}

		private static void InitializeMacro (MemberInfo member)
		{
			if (!Macro.IsDefined (member))
				InitializeStaticMember (member);
		}

		private static void InitializeStaticMember (MemberInfo member)
		{
			var foo = member is FieldInfo ?
				(member as FieldInfo).GetValue (null) :
				(member as PropertyInfo).GetValue (null);
		}

		protected static Tuple<Type, int> GetArrayLen (MemberInfo member, Type memberType)
		{
			return memberType.IsArray ?
				Tuple.Create (memberType.GetElementType (), member.ExpectFixedArrayAttribute ().Length) :
				Tuple.Create (memberType, 0);
		}

		protected MethodCallExpression CastFromBinding (Expression expr)
		{
			var me = expr.CastExpr<MethodCallExpression> (ExpressionType.Call);
			return  me != null && me.Method.IsLiftMethod () ? me : null;
		}

        protected bool FromBinding (Source source)
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

        protected bool LetBinding (Source source)
        {
            return source.ParseLambda ((_, ne) => OutputLet (ne));
        }

		protected bool OutputLet (NewExpression ne)
        {
            for (int i = 0; i < ne.Members.Count; i++)
            {
                var prop = (PropertyInfo)ne.Members[i];
				if (!(prop.Name.StartsWith ("<>") || ne.Arguments[i] is ParameterExpression))
				{
					var expr = ne.Arguments[i];
					if (prop.Name != expr.ToString ())
						OutputMacroExpandedLocalVar (prop.PropertyType, prop.Name, expr);
				}
            }
            return true;
        }

		protected void OutputMacroExpandedLocalVar (Type type, string name, Expression expr)
		{
			var local = Ast.Var (type, name);
			var expanded = ExtractMacros (expr, local);
			if (_currentScope.FindLocalVar (local.Name) == null)
				_currentScope.DeclareLocal (local, Expr (expanded));
		}

		protected Expression ExtractMacro (InvocationExpression ie, Expression rootExpression,
			Ast.Variable resultVar)
		{
			Ast.MacroDefinition macro = null;
			var me = ie.Expression.CastExpr<MemberExpression> (ExpressionType.MemberAccess);
			if (me != null)
			{
				InitializeMacro (me.Member);
				var mac = Macro.Get (me.Member);
				if (!Macro.IsMacroType (me.Type) || mac == null)
					return ie;
				AddExternalReferences (mac.Program, mac.TypesDefined);
				macro = mac.AstMacro;
			}
			else
			{
				var pe = ie.Expression.CastExpr<ParameterExpression> (ExpressionType.Parameter);
				if (pe == null || !Macro.IsMacroType (pe.Type))
					return ie;
				macro = MacroDefParam (pe);
			}
			if (ie != rootExpression || resultVar == null)
				resultVar = Macro.GenUniqueVar (macro.Result.Type, "res");
			_currentScope.DeclareLocal (resultVar, null);
			_currentScope.CodeOut (Ast.MCall (macro, resultVar, ie.Arguments.Select (MacroParam)));
			return Expression.Parameter (ie.Type, resultVar.Name);
		}

		private Ast.MacroDefinition MacroDefParam (ParameterExpression pe)
		{
			var mpar = _currentScope.FindMacroParam (pe);
			if (mpar == null)
				throw new ParseException (string.Format ("Macro parameter '{0}' not in scope.", pe.Name));
			return ((Ast.MacroDefParam)mpar).Definition;
		}

		protected Ast MacroParam (Expression expr)
		{
			if (expr is LambdaExpression)
				return ParseMacro (expr as LambdaExpression);
			if (expr is ParameterExpression && Macro.IsMacroType (expr.Type))
				return MacroDefParam (expr as ParameterExpression);
			else
				return Expr (ExtractMacros (expr, null));
		}

		protected Expression ExtractMacros (Expression expr, Ast.Variable returnVar)
		{
			return expr.ReplaceSubExpression<InvocationExpression> (ExpressionType.Invoke, 
				e => ExtractMacro (e, expr, returnVar));
		}

		protected bool Where (Source source)
		{
			if (!source.Current.Method.IsWhere (_linqType))
				return false;
			var predicate = source.Current.Arguments[1].ExpectLambda ().Body;
			_currentScope.CodeOut (Ast.If (
				Ast.Op (MapOperator (null, ExpressionType.Not), Expr (predicate)),
				Ast.Ret ()));
			return true;
		}

        protected void Return (Expression expr)
        {
			OutputReturn (ExtractMacros (expr, null));
        }

		protected virtual void OutputReturn (Expression expr)
		{
			var ne = expr.CastExpr<NewExpression> (ExpressionType.New);
			if (ne == null)
			{
				var mie = expr.CastExpr<MemberInitExpression> (ExpressionType.MemberInit);
				if (mie == null)
					throw new ParseException (
						"The select clause must be a 'new' expression. Currently it is: " + expr);
				foreach (MemberAssignment assign in mie.Bindings)
					OutputReturnAssignment (assign.Member, assign.Expression);
			}
			else
			{
				for (int i = 0; i < ne.Members.Count; i++)
				{
					var prop = (PropertyInfo)ne.Members[i];
					if (!prop.Name.StartsWith ("<>"))
						OutputReturnAssignment (prop, ne.Arguments[i]);
				}
			}
		}

		protected void OutputReturnAssignment (MemberInfo member, Expression expr)
		{
			var memVar = Ast.Var (expr.Type, member.Name);
			_currentScope.CodeOut (Ast.Ass (Ast.VRef (memVar), Expr (expr)));
		}

		protected void ConditionalReturn (Expression expr, Action<Expression> returnAction)
		{
			var ce = expr.CastExpr<ConditionalExpression> (ExpressionType.Conditional);
			if (ce == null)
				returnAction (expr);
			else
			{
				var thenBlock = Ast.Blk ();
				var elseBlock = Ast.Blk ();
				_currentScope.CodeOut (Ast.If (Expr (ce.Test), thenBlock, elseBlock));
				BeginScope (thenBlock);
				returnAction (ce.IfTrue);
				EndScope ();
				BeginScope (elseBlock);
				ConditionalReturn (ce.IfFalse, returnAction);
				EndScope ();
			}
		}

		protected Expression ParseLinqExpression (Expression expr)
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

		protected void FunctionBody (Expression expr)
		{
			var node = expr.CastExpr<MethodCallExpression> (ExpressionType.Call);
			_currentScope.CodeOut (Ast.Ret (Expr (ExtractMacros (
				node != null && node.Method.IsEvaluate (_linqType) ? 
					ParseLinqExpression (node.Arguments [0]) : 
					expr, null))));
		}

		protected void MacroBody (Ast.MacroDefinition definition, Expression expr)
		{
			var node = expr.CastExpr<MethodCallExpression> (ExpressionType.Call);
			_currentScope.CodeOut (Ast.Ass (Ast.VRef (definition.Result),
                Expr (ExtractMacros (
					node != null && node.Method.IsEvaluate (_linqType) ?
						ParseLinqExpression (node.Arguments[0]) :
						expr, null))));
		}
	}
}
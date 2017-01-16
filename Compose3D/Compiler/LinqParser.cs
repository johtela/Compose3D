﻿namespace Compose3D.Compiler
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
		internal static Dictionary<string, CompiledFunction> _functions = 
			new Dictionary<string, CompiledFunction> ();
		protected Ast.Program _program;
		protected Ast.Function _function;
		protected HashSet<Type> _typesDefined;
		protected Stack<Ast.Block> _scopes;
		protected Dictionary<string, Ast.Variable> _locals;
		protected Dictionary<string, Ast.Constant> _constants;
		protected Dictionary<string, Ast.Variable> _globals;
		protected Type _linqType;
		protected TypeMapping _typeMapping;

		protected LinqParser (Type linqType, TypeMapping typeMapping)
        {
            _typesDefined = new HashSet<Type> ();
			_scopes = new Stack<Ast.Block> ();
			_locals = new Dictionary<string, Ast.Variable> ();
			_constants = new Dictionary<string, Ast.Constant> ();
			_globals = new Dictionary<string, Ast.Variable> ();
			_linqType = linqType;
			_typeMapping = typeMapping;
			_program = Ast.Prog ();
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

		protected virtual Ast.Expression MapMemberAccess (MemberExpression me)
		{
			Ast.Variable v;
			if (_locals.TryGetValue (me.Member.Name, out v))
				return Ast.VRef (v);
			if (_globals.TryGetValue (me.Member.Name, out v))
				return Ast.VRef (v);
			Ast.Constant c;
			if (_constants.TryGetValue (me.Member.Name, out c))
				return Ast.VRef (c);
			throw new ParseException ("Access to undefined member " + me.Member.Name);
		}

		protected abstract string MapTypeCast (Type type);

		protected abstract void OutputFromBinding (ParameterExpression par, MethodCallExpression node);

		protected static void CreateFunction (LinqParser parser, string name, LambdaExpression expr)
		{
			parser.Function (name, expr);
			parser._program.Functions.Add (parser._function);
			_functions.Add (name, new CompiledFunction (parser._program, parser._function, 
				parser._typesDefined));
		}

		public static string ConstructFunctionName (MemberInfo member)
		{
			return member.DeclaringType.Name + "_" + member.Name;
		}

		protected void Function (string name, LambdaExpression expr)
		{
			var args = (from p in expr.Parameters
						select Ast.Arg (MapType (p.Type), p.Name)).ToArray ();
			var scope = Ast.Blk ();
			_function = Ast.Fun (name, MapType (expr.ReturnType), args, scope);
			foreach (var arg in args)
				_locals.Add (arg.Name, arg);
			StartScope (scope);
			FunctionBody (expr.Body);
			EndScope ();
		}

		protected Ast.Block CurrentScope ()
		{
			return _scopes.Peek ();
		}

		protected void StartScope (Ast.Block block)
		{
			_scopes.Push (block);
		}

		protected void EndScope ()
		{
			_scopes.Pop ();
		}

		protected void CodeOut (Ast.Statement statement)
		{
			CurrentScope ().Statements.Add (statement);
		}

		protected void DeclOut (string declaration, params object[] args)
		{
			_program.AddGlobal (Ast.Decl (args.Length == 0 ?
				declaration :
				string.Format (declaration, args)));
		}

		protected Ast.Variable DeclareLocal (string type, string name, Ast.Expression value)
		{
			var local = Ast.Var (type, name);
			CodeOut (Ast.DeclVar (local, value));
			_locals.Add (local.Name, local);
			return local;
		}

		protected Ast.Variable NewIndexVar (string type, string name)
		{
			var i = 0;
			string result;
			do
				result = string.Format ("_gen_{0}{1}", name, i++);
			while (_locals.ContainsKey (result));
			return Ast.Var (type, result);
		}

		protected bool DefineType (Type type)
		{
			if (_typesDefined.Contains (type))
				return false;
			_typesDefined.Add (type);
			return true;
		}

		protected Ast.Expression Expr (Expression expr)
		{
			var result =
				expr.Match<BinaryExpression, Ast.Expression> (be =>
					Ast.Op (MapOperator (be.Method, be.NodeType), Expr (be.Left), Expr (be.Right)))
				??
				expr.Match<UnaryExpression, Ast.Expression> (ue =>
					Ast.Op (
						ue.NodeType == ExpressionType.Convert ?
							MapTypeCast (ue.Type) :
							MapOperator (ue.Method, ue.NodeType),
						Expr (ue.Operand)))
				??
				expr.Match<MethodCallExpression, Ast.Expression> (mc =>
				{
					if (mc.Method.Name == "get_Item")
					{
						if (mc.Arguments[0].Type == typeof (Maths.Coord))
							return Ast.FRef (Expr (mc.Object),
								Ast.Fld (mc.Arguments.Select (a => Expr (a).ToString ()).SeparateWith ("")));
						var syntax = _typeMapping.Indexer (mc.Method);
						return syntax == null || mc.Arguments.Count != 1 ? 
							null :
							Ast.Op (syntax, Expr (mc.Object), Expr (mc.Arguments[0]));
					}
					var args = mc.Method.IsStatic ? mc.Arguments : mc.Arguments.Prepend (mc.Object);
					return Ast.Call (MapFunction (mc.Method), args.Select (Expr));
                }) ??
				expr.Match<InvocationExpression, Ast.Expression> (ie =>
				{
					var me = ie.Expression.CastExpr<MemberExpression> (ExpressionType.MemberAccess);
					if (me != null && !me.Type.IsMacroType ())
						return FunctionCall (ie, me.Member);
					//var pe = ie.Expression.CastExpr<ParameterExpression> (ExpressionType.Parameter);
					//if (pe != null)
					//	return OutputDelegateCall (ie, pe);
					throw new ParseException ("Delegate called must be either a member of a class, or a parameter.");
				}) ??
                expr.Match<MemberExpression, Ast.Expression> (MapMemberAccess)
                ??
                expr.Match<NewExpression, Ast.Expression> (ne =>
					Ast.Call (MapConstructor (ne.Constructor), ne.Arguments.Select (Expr)))
                ??
                expr.Match<ConstantExpression, Ast.Expression> (ce => 
					Ast.Lit (string.Format (CultureInfo.InvariantCulture, 
						ce.Type == typeof (float) ?  "{0:0.0############}f" : "{0}", ce.Value))) 
				?? 
				expr.Match<NewArrayExpression, Ast.Expression> (na => 
					Ast.Arr (MapType (na.Type.GetElementType ()), na.Expressions.Count,
						na.Expressions.Select (Expr))) 
				??
                expr.Match<ConditionalExpression, Ast.Expression> (ce => 
					Ast.Cond (Expr (ce.Test), Expr (ce.IfTrue), Expr (ce.IfFalse)))
				?? 
                expr.Match<ParameterExpression, Ast.Expression> (pe =>
				{
					Ast.Variable local;
					if (!_locals.TryGetValue (pe.Name, out local))
						throw new ParseException ("Reference to undefined local variable: " + pe.Name);
					return Ast.VRef (local);
				}) 
				?? 
				null;
            if (result == null)
                throw new ParseException (string.Format ("Unsupported expression type {0}", expr));
            return result;
        }

		private Ast.FunctionCall FunctionCall (InvocationExpression ie, MemberInfo member)
		{
			var name = ConstructFunctionName (member);
			CompiledFunction fun;
			if (_functions.TryGetValue (name, out fun))
			{
				if (!_program.Functions.Contains (fun.Function))
				{
					_typesDefined.UnionWith (fun.TypesDefined);
					foreach (var glob in fun.Program.Globals)
						if (!_program.GlobalDictionary.ContainsKey (glob.Name))
							_program.AddGlobal (glob);
					foreach (var func in fun.Program.Functions)
						if (!_program.Functions.Contains (func))
							_program.Functions.Add (func);
				}
				return Ast.Call (fun.Function, ie.Arguments.Select (Expr));
			}
			throw new ParseException ("Undefined function: " + name);
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
					var type = MapType (prop.PropertyType);
					var expr = ne.Arguments[i];
					if (prop.Name != expr.ToString ())
						DeclareLocal (type, prop.Name, Expr (RemoveAggregates (expr)));
				}
            }
            return true;
        }

		protected Expression RemoveAggregates (Expression expr)
		{
			return expr.ReplaceSubExpression<MethodCallExpression> (ExpressionType.Call, Aggregate);
		}

        protected Expression Aggregate (MethodCallExpression expr)
        {
            if (!expr.Method.IsAggregate ())
                return expr;
            var aggrFun = expr.Arguments[2].Expect<LambdaExpression> (ExpressionType.Lambda);
            var accum = aggrFun.Parameters[0];
            var iterVar = aggrFun.Parameters[1];
			var al = DeclareLocal (MapType (accum.Type), accum.Name, Expr (expr.Arguments[1]));
			var se = expr.Arguments[0].GetSelect (_linqType);
			if (se != null)
			{
				ParseFor (se);
				DeclareLocal (MapType (iterVar.Type), iterVar.Name,
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
			CodeOut (Ast.Ass (Ast.VRef (al), Expr (aggrFun.Body)));
			EndScope ();
            return accum;
        }

		protected void ParseFor (MethodCallExpression mce)
		{
			if (mce.Arguments[0].GetSelect (_linqType) == null)
				IterateArray (mce);
			else
				Parse.ExactlyOne (ForLoop).IfFail (new ParseException (
					"Must have exactly one from clause in the beginning of aggregate expression."))
					.Then (Parse.ZeroOrMore (LetBinding))
					.Execute (new Source (mce.Arguments[0].Traverse (_linqType)));
		}

		private Ast.ForLoop ForStatement (Ast.Variable indexVar, Ast.Expression len, Ast.Block loopBlock)
		{
			return Ast.For (indexVar, Ast.Lit ("0"),
				Ast.Op (MapOperator (null, ExpressionType.LessThan), Ast.VRef (indexVar), len),
			   Ast.Op (MapOperator (null, ExpressionType.PostIncrementAssign), Ast.VRef (indexVar)),
			   loopBlock);
		}

		protected void IterateArray (MethodCallExpression expr)
        {
			var array = expr.Arguments[0];
            var member  = array.SkipUnary (ExpressionType.Not)
                .Expect<MemberExpression> (ExpressionType.MemberAccess).Member;
			var len = 0;
			var field = member as FieldInfo;
			if (field != null)
				len = field.ExpectFixedArrayAttribute ().Length;
			else if (_constants.ContainsKey (member.Name))
				len = _constants[member.Name].ArrayLength;
			else
				throw new ParseException ("Invalid array expression. " +
					"Expected uniform field reference or constant array. Encountered: " + array);
			var indexVar = NewIndexVar ("int", "ind");
			var item = expr.Method.IsSelect (_linqType) ?
				expr.GetSelectLambda ().Parameters[0] :
				expr.Arguments[2].ExpectLambda ().Parameters[1];
			var loopBlock = Ast.Blk ();
			CodeOut (ForStatement (indexVar, Ast.Lit (len.ToString ()), loopBlock));
			StartScope (loopBlock);
			DeclareLocal (MapType (item.Type), item.Name,
				Ast.Op (MapOperator (null, ExpressionType.ArrayIndex), Expr (array), Ast.VRef (indexVar)));
        }

		protected void OutputForLoop (MethodCallExpression expr)
		{
			var indexVar = expr.Method.IsSelect (_linqType) ?
				expr.GetSelectLambda ().Parameters[0] :
				expr.Arguments[2].ExpectLambda ().Parameters[1];
			var range = expr.Arguments[0].Expect<MethodCallExpression> (ExpressionType.Call);
			var start = Expr (range.Arguments[0]);
			var iv = Ast.Var (MapType (indexVar.Type), indexVar.Name);
			_locals.Add (indexVar.Name, iv);
			var loopBlock = Ast.Blk ();
			if (range.Method.DeclaringType == typeof (Enumerable))
			{
				var len = Expr (range.Arguments[1]);
				CodeOut (ForStatement (iv, len, loopBlock));
			}
			else
			{
				var end = Expr (range.Arguments[1]);
				var step = Expr (range.Arguments[2]);
				CodeOut (Ast.For (iv, start, 
					Ast.Op (MapOperator (null, ExpressionType.NotEqual), Ast.VRef (iv), end),
					Ast.Op (MapOperator (null, ExpressionType.AddAssign), Ast.VRef (iv), step),
					loopBlock));
			}
			StartScope (loopBlock);
		}

		protected bool ForLoop (Source source)
		{
			var se = source.Current;
			IterateArray (se);
			OutputLet (se.GetSelectLambda ().Body.Expect<NewExpression> (ExpressionType.New));
			return true;
		}

		protected bool Where (Source source)
		{
			if (!source.Current.Method.IsWhere (_linqType))
				return false;
			var predicate = source.Current.Arguments[1].ExpectLambda ().Body;
			CodeOut (Ast.If (Ast.Op (MapOperator (null, ExpressionType.Not), Expr (predicate)),
				Ast.Ret ()));
			return true;
		}

        protected void Return (Expression expr)
        {
			expr = RemoveAggregates (expr);
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

		protected virtual void OutputReturnAssignment (MemberInfo member, Expression expr)
		{
			var memVar = Ast.Var (MapType (expr.Type), member.Name);
			CodeOut (Ast.Ass (Ast.VRef (memVar), Expr (expr)));
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
				CodeOut (Ast.If (Expr (ce.Test), thenBlock, elseBlock));
				StartScope (thenBlock);
				returnAction (ce.IfTrue);
				EndScope ();
				StartScope (elseBlock);
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
			CodeOut (Ast.Ret (Expr (RemoveAggregates (
				node != null && node.Method.IsEvaluate (_linqType) ? 
					ParseLinqExpression (node.Arguments [0]) : 
					expr))));
		}
	} 
}
namespace Compose3D.CLTypes
{
    using System;
	using System.Collections.Generic;
    using System.Linq;
	using System.Linq.Expressions;
    using System.Reflection;
    using Extensions;
	using Compiler;
	using Parallel;

	public class ClcParser : LinqParser
	{
		private ClcParser () : base (typeof (Kernel), new CLTypeMapping ())
		{ }

		public static string CompileKernels (params CLKernel[] kernels)
		{
			var parser = new ClcParser ();
			foreach (var kernel in kernels)
			{
				var body = Ast.Blk ();
				parser.BeginScope (body);
				parser._function = ClcAst.Kern (kernel._name,
					parser.KernelArguments (kernel._expr.Parameters), body);
				parser.OutputKernel (kernel._expr);
			}
			return parser.BuildKernelCode ();
		}

		protected override IEnumerable<Ast.Argument> FunctionArguments (IEnumerable<ParameterExpression> pars)
		{
			return pars.Select (p => KernelArgument (p.Type, p.Name));
		}

		private IEnumerable<ClcAst.KernelArgument> KernelArguments (IEnumerable<ParameterExpression> pars)
		{
			foreach (var par in pars)
				if (par.Type.IsSubclassOf (typeof (ArgGroup)))
					foreach (var mem in ArgGroup.MemberDefinitions (par.Type, par.Name + "_"))
						yield return (ClcAst.KernelArgument)KernelArgument (mem.Item2, mem.Item1);
				else
					yield return (ClcAst.KernelArgument)KernelArgument (par.Type, par.Name);
		}

		private Ast.Argument KernelArgument (Type type, string name)
		{
			Ast.Argument result;
			if (type.IsSubclassOf (typeof (KernelArg)))
			{
				var typeDef = type.GetGenericTypeDefinition ();
				var elemType = type.GetGenericArguments ()[0];
				result = typeDef == typeof (Value<>) ?
						ClcAst.KArg (elemType, name, ClcAst.KernelArgumentKind.Value) :
					typeDef == typeof (Buffer<>) ?
						ClcAst.KArg (elemType, name, ClcAst.KernelArgumentKind.Buffer,
							ClcAst.KernelArgumentMemory.Global) :
						null;
				if (result == null)
					throw new ParseException ("Argument groups are only valid as kernel arguments.");
			}
			else
				result = Ast.Arg (type, name);
			_currentScope.AddLocal (name, result);
			return result;
		}

		public static void CreateFunction (MemberInfo member, LambdaExpression expr)
		{
			CreateFunction (new ClcParser (), member, expr);
		}

		public static void CreateMacro (MemberInfo member, LambdaExpression macro)
		{
			CreateMacro (new ClcParser (), member, macro);
		}

		private string BuildKernelCode ()
		{
			_program.Functions.Add (Macro.InstantiateAllMacros (_function));
			return _program.Output (this);
		}

		protected override Ast.Expression MapMemberAccess (MemberExpression me)
		{
			var clfield = me.Member.GetAttribute<CLFieldAttribute> ();
			if (clfield != null)
				return Ast.FRef (Expr (me.Expression), Ast.Fld (clfield.Name));
			var syntax = me.Member.GetCLSyntax ();
			if (syntax != null)
				return Ast.Call (syntax, Expr (me.Expression));
			var declType = me.Expression.Type;
			if (declType.IsCLStruct ()  || declType.IsCLUnion ())
			{
				MapType (declType);
				var astruct = (Ast.Structure)_globals[StructTypeName (declType)];
				var field = astruct.Fields.Find (f => f.Name == me.Member.Name);
				return Ast.FRef (Expr (me.Expression), field);
			}
			if (declType.IsSubclassOf (typeof (ArgGroup)))
			{
				var argName = ArgGroupMemberName (me);
				Ast.Variable v = _currentScope.FindLocalVar (argName);
				if (v == null)
					throw new ParseException ("Group argument not defined: " + argName);
				return Ast.VRef (v);
			}
			return base.MapMemberAccess (me);
		}

		private string ArgGroupMemberName (Expression expr)
		{
			var pe = expr.CastExpr<ParameterExpression> (ExpressionType.Parameter);
			if (pe != null)
				return pe.Name;
			var me = expr.Expect<MemberExpression> (ExpressionType.MemberAccess);
			return ArgGroupMemberName (me.Expression) + "_" + me.Member.Name;
		}

		protected override string MapTypeCast (Type type)
		{
			return string.Format ("({0}){{0}}", MapType (type));
		}

		protected internal override string MapType (Type type)
		{
			if (type.IsCLStruct () || type.IsCLUnion ())
			{
				DefineStruct (type, type.IsCLUnion ());
				return StructTypeName (type);
			}
			return base.MapType (type);
		}

		private void DefineStruct (Type structType, bool isUnion)
		{
			if (!DefineType (structType)) return;
			var name = StructTypeName (structType);
			var fields = from field in structType.GetCLFields ()
						 let fi = GetArrayLen (field, field.FieldType)
						 select Ast.Fld (fi.Item1, field.Name, fi.Item2);
			AddGlobal (isUnion ?
				ClcAst.Union (name, fields) :
				ClcAst.Struct (name, fields));
		}

		private void DeclareConstants (Expression expr)
		{
			var ne = expr.CastExpr<NewExpression> (ExpressionType.New);
			if (ne == null)
			{
				var mie = expr.CastExpr<MemberInitExpression> (ExpressionType.MemberInit);
				if (mie == null)
					throw new ParseException ("Unsupported kernel expression: " + expr);
				foreach (MemberAssignment assign in mie.Bindings)
				{
					var memberType = assign.Member is FieldInfo ?
						(assign.Member as FieldInfo).FieldType :
						(assign.Member as PropertyInfo).PropertyType;
					DeclareConstant (memberType, assign.Member.Name, assign.Expression);
				}
			}
			else
			{
				for (int i = 0; i < ne.Members.Count; i++)
				{
					var prop = (PropertyInfo)ne.Members[i];
					if (!prop.Name.StartsWith ("<>"))
						DeclareConstant (prop.PropertyType, prop.Name, ne.Arguments[i]);
				}
			}
		}

		private void DeclareConstant (Type constType, string name, Expression value)
		{
			Ast.Constant con;
			if (constType.IsArray)
			{
				var nai = value.Expect<NewArrayExpression> (ExpressionType.NewArrayInit);
				con = Ast.Const (constType.GetElementType (), name, nai.Expressions.Count, Expr (value));
			}
			else
				con = Ast.Const (constType, name, Expr (value));
			AddGlobal (ClcAst.DeclConst (con));
			_constants.Add (name, con);
		}

		protected override void OutputFromBinding (ParameterExpression par, MethodCallExpression node)
		{
			var type = node.Method.GetGenericArguments ()[0];
			if (node.Method.Name == "Constants")
				DeclareConstants (node.Arguments[0]);
			else if (node.Method.Name == "ToKernel")
				OutputMacroExpandedLocalVar (type, par.Name, node.Arguments[0]);
			else
				throw new ArgumentException ("Unsupported lift method.", node.Method.ToString ());
		}

		protected override Ast.NewArray ArrayConstant (Type type, int count, IEnumerable<Ast.Expression> items)
		{
			return ClcAst.Arr (type, count, items);
		}

		protected override Ast.InitStruct InitStructure (Type type,
			IEnumerable<Tuple<Ast.VariableRef, Ast.Expression>> initFields)
		{
			return ClcAst.InitS (type, initFields);
		}

		protected override void OutputReturn (Expression expr)
		{
			var lie = expr.Expect<ListInitExpression> (ExpressionType.ListInit);
			var res = _function.Arguments.Last ();
			foreach (var init in lie.Initializers)
			{
				var assign = init.Arguments[0].Expect<MethodCallExpression> (ExpressionType.Call);
				var args = assign.Arguments;
				if (assign.Method.Name == "Buffer")
				{
					var buf = args[0].Expect<ParameterExpression> (ExpressionType.Parameter);
					var bufVar = _currentScope.FindLocalVar (buf.Name);
					_currentScope.CodeOut (Ast.Ass (Ast.ARef (bufVar, Expr (args[1])), Expr (args[2])));
				}
				else
					throw new ParseException ("Invalid assign method.");
			}
		}

		private void OutputKernel (LambdaExpression expr)
		{
			var retExpr = ParseLinqExpression (expr.Body);
			ConditionalReturn (retExpr, Return);
			EndScope ();
		}
	}
}
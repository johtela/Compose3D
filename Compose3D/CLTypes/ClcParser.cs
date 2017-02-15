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
					kernel._expr.Parameters.Select (parser.KernelArgument)
						.Append (KernelResult (kernel._expr.ReturnType)),
					body);
				parser.OutputKernel (kernel._expr);
			}
			return parser.BuildKernelCode ();
		}

		private ClcAst.KernelArgument KernelArgument (ParameterExpression par)
		{
			var typeDef = par.Type.GetGenericTypeDefinition ();
			var elemType = par.Type.GetGenericArguments ()[0];
			var result = typeDef == typeof (Value<>) ? 
					ClcAst.KArg (elemType, par.Name, ClcAst.KernelArgumentKind.Value) :
				typeDef == typeof (Buffer<>) ?
					ClcAst.KArg (elemType, par.Name, ClcAst.KernelArgumentKind.Buffer) :
					null;
			if (result == null)
				throw new ArgumentException ("Invalid argument type");
			_currentScope.AddLocal (par.Name, result);
			return result;
		}

		private static ClcAst.KernelArgument KernelResult (Type type)
		{
			var elemType = type.GetGenericArgument (0, 0);
			return ClcAst.KArg (elemType, "result", ClcAst.KernelArgumentKind.Buffer); 
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
			if (declType.IsCLStruct ())
			{
				var field = Ast.Fld (me.Type, me.Member.Name);
				return Ast.FRef (Expr (me.Expression), field);
			}
			return base.MapMemberAccess (me);
		}

		protected override string MapTypeCast (Type type)
		{
			return string.Format ("({0}){{0}}", MapType (type));
		}

		//protected override string MapType (Type type)
		//{
		//	if (type.IsGLStruct ())
		//		OutputStruct (type);
		//	return base.MapType (type);
		//}

		//private void OutputStruct (Type structType)
		//{
		//	if (!DefineType (structType)) return;
		//	foreach (var field in structType.GetGLFields ())
		//		if (field.FieldType.IsGLStruct ())
		//			OutputStruct (field.FieldType);
		//	DeclOut ("struct {0}\n{{", structType.Name);
		//	foreach (var field in structType.GetGLFields ())
		//		DeclareVariable (field, field.FieldType, "    ");
		//	DeclOut ("};");
		//}

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
				_currentScope.DeclareLocal (type, par.Name, Expr (node.Arguments[0]));
			else
				throw new ArgumentException ("Unsupported lift method.", node.Method.ToString ());
		}

		protected override Ast.NewArray ArrayConstant (Type type, int count, IEnumerable<Ast.Expression> items)
		{
			return ClcAst.Arr (type, count, items);
		}

		protected override void OutputReturn (Expression expr)
		{
			var lie = expr.Expect<ListInitExpression> (ExpressionType.ListInit);
			var bufType = lie.Type.GetGenericArguments ()[0];
			var res = _function.Arguments.Last ();
			foreach (var init in lie.Initializers)
			{
				var args = init.Arguments;
				_currentScope.CodeOut (Ast.Ass (Ast.ARef (res, Expr (args[0])), Expr (args[1])));
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
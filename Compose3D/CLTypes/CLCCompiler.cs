﻿namespace Compose3D.CLTypes
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text.RegularExpressions;
	using Extensions;
	using Compiler;
	using Parallel;

	public class CLCCompiler : LinqCompiler
	{
		private CLArguments _arguments;

		private CLCCompiler (CLArguments arguments) : 
			base (typeof (Kernel), new CLTypeMapping ())
		{
			_arguments = arguments;
		}

		public static string CreateKernel<T> (string name, 
			Expression<Func<Kernel<T>>> kernel, CLArguments arguments)
		{
			var compiler = new CLCCompiler (arguments);
			compiler.OutputKernel (kernel);
			return BuildKernelCode (compiler);
		}

		public static void CreateFunction (MemberInfo member, LambdaExpression expr)
		{
			CreateFunction (new CLCCompiler (null), member, expr);
		}

		private static string BuildKernelCode (CLCCompiler compiler)
		{
			return compiler._decl.ToString () +
				GenerateFunctions (compiler._funcRefs) +
				compiler._code.ToString ();
		}

		protected override string MapMemberAccess (MemberExpression me)
		{
			var syntax = me.Member.GetCLSyntax ();
			return syntax != null ?
				string.Format (syntax, Expr (me.Expression)) :
				me.Expression.Type.IsCLType () ?
					string.Format ("{0}.{1}", Expr (me.Expression), me.Member.GetCLFieldName ()) :
					me.Member.Name;
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

		//private void DeclareConstants (Expression expr)
		//{
		//	var ne = expr.CastExpr<NewExpression> (ExpressionType.New);
		//	if (ne == null)
		//	{
		//		var mie = expr.CastExpr<MemberInitExpression> (ExpressionType.MemberInit);
		//		if (mie == null)
		//			throw new ParseException ("Unsupported shader expression: " + expr);
		//		foreach (MemberAssignment assign in mie.Bindings)
		//		{
		//			var memberType = assign.Member is FieldInfo ?
		//				(assign.Member as FieldInfo).FieldType :
		//				(assign.Member as PropertyInfo).PropertyType;
		//			OutputConst (memberType, assign.Member.Name, assign.Expression);
		//		}
		//	}
		//	else
		//	{
		//		for (int i = 0; i < ne.Members.Count; i++)
		//		{
		//			var prop = (PropertyInfo)ne.Members[i];
		//			if (!prop.Name.StartsWith ("<>"))
		//				OutputConst (prop.PropertyType, prop.Name, ne.Arguments[i]);
		//		}
		//	}
		//}

		//private void OutputConst (Type constType, string name, Expression value)
		//{
		//	_constants.Add (name, new Constant (constType, name, value));
		//	if (constType.IsArray)
		//	{
		//		var elemType = constType.GetElementType ();
		//		var elemGLType = MapType (elemType);
		//		var nai = value.Expect<NewArrayExpression> (ExpressionType.NewArrayInit);
		//		CodeOut ("const {0} {1}[{2}] = {0}[] (\n\t{3});",
		//			elemGLType, name, nai.Expressions.Count,
		//			nai.Expressions.Select (Expr).SeparateWith (",\n\t"));
		//	}
		//	else
		//		CodeOut ("const {0} {1} = {2};", MapType (constType), name, Expr (value));
		//}

		protected override void OutputFromBinding (ParameterExpression par, MethodCallExpression node)
		{
			if (node.Method.Name == "State")
				return;
			var type = node.Method.GetGenericArguments ()[0];
			if (node.Method.Name == "Argument")
				_arguments.Add (par.Name, type, CLArgumentKind.Value);
			else if (node.Method.Name == "Buffer")
				_arguments.Add (par.Name, type, CLArgumentKind.Buffer);
			//		DeclareUniforms (type);
			//	else if (node.Method.Name == "Constants")
			//		DeclareConstants (node.Arguments [0]);
			//	else if (node.Method.Name == "ToShader")
			//		CodeOut ("{0} {1} = {2};", MapType (type), par.Name, Expr (node.Arguments [0]));
			//	else
			//		throw new ArgumentException ("Unsupported lift method.", node.Method.ToString ());
		}

		private void OutputKernel (LambdaExpression expr)
		{
			StartMain ();
			var retExpr = ParseLinqExpression (expr.Body);
			ConditionalReturn (retExpr, Return);
			EndFunction ();
		}
	}
}
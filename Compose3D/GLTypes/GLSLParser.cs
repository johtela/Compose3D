﻿namespace Compose3D.GLTypes
{
    using OpenTK.Graphics.OpenGL4;
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text.RegularExpressions;
	using Extensions;
	using Compiler;
	using Shaders;

	public class GLSLParser : LinqParser
    {
		private GLSLParser () : base (typeof (Shader), new GLTypeMapping ())
        { }

		public static string CreateShader<T> (string version, Expression<Func<Shader<T>>> shader)
		{
			var parser = new GLSLParser ();
			parser.DeclareVariables (typeof (T), "out", "");
			parser.OutputShader (shader);
			return parser.BuildShaderCode ();
		}

		public static string CreateShader<T> (Expression<Func<Shader<T>>> shader)
		{
			return CreateShader<T> (GetGLSLVersion ().ToString (), shader);
		}

		public static string CreateGeometryShader<T> (string version, int vertexCount,
			int invocations, PrimitiveType inputPrimitive, PrimitiveType outputPrimitive,
			Expression<Func<Shader<T[]>>> shader)
		{
			var parser = new GLSLParser ();
			if (invocations > 0)
				parser.DeclOut ("layout (invocations = {0}) in;", invocations);
			parser.DeclOut ("layout ({0}) in;", inputPrimitive.MapInputGSPrimitive ());
			parser.DeclOut ("layout ({0}, max_vertices = {1}) out;", 
				outputPrimitive.MapOutputGSPrimitive (), vertexCount);
			parser.DeclareVariables (typeof (T), "out", "");
			parser.OutputGeometryShader (shader);
			return parser.BuildShaderCode ();
		}

		public static string CreateGeometryShader<T> (int vertexCount, int invocations,
			PrimitiveType inputPrimitive, PrimitiveType outputPrimitive,
			Expression<Func<Shader<T[]>>> shader)
		{
			var currVersion = GetGLSLVersion ();
			var minVersion = invocations == 0 ? 150 : 400;
			var version = currVersion > minVersion ? currVersion : minVersion;
			return CreateGeometryShader<T> (version.ToString (), vertexCount, invocations, 
				inputPrimitive, outputPrimitive, shader);
		}

		public static void CreateFunction (MemberInfo member, LambdaExpression expr)
		{
			var name = ConstructFunctionName (member);
			CreateFunction (new GLSLParser (), name, expr);
		}

		private string BuildShaderCode ()
		{
			AstTransform.IncludeCalledFunctions (_function, _program);
			return "#version 400 core\nprecision highp float;\n" + _program.ToString ();
		}

		private static int GetGLSLVersion ()
		{
			var glslVersion = GL.GetString (StringName.ShadingLanguageVersion);
			var match = new Regex (@"(\d+)\.([^\-]+).*").Match (glslVersion);
			if (!match.Success || match.Groups.Count != 3)
				throw new ParseException ("Invalid GLSL version string: " + glslVersion);
			return int.Parse (match.Groups[1].Value + match.Groups[2].Value);
		}

		protected override Ast.Expression MapMemberAccess (MemberExpression me)
		{
			var syntax = me.Member.GetGLSyntax ();
			if (syntax != null)
				return Ast.Call (syntax, Expr (me.Expression));
			var structType = me.Expression.Type;
			if (_typesDefined.Contains (structType))
			{
				var structDef = _program.FindStruct (structType.Name);
				var field = structDef.FindField (me.Member.GetGLFieldName ());
				return Ast.FRef (Expr (me.Expression), field);
			}
			return base.MapMemberAccess (me);
		}

		protected override string MapTypeCast (Type type)
		{
			return string.Format ("{0} ({{0}})", MapType (type));
		}

		protected override string MapType (Type type)
		{
			if (type.IsGLStruct ())
				DefineStruct (type);
			return base.MapType (type);
		}

		private void DefineStruct (Type structType)
		{
			if (!DefineType (structType)) return;
			_program.Globals.Add (Ast.Struct (structType.Name,
				from field in structType.GetGLFields ()
				select Ast.Fld (MapType (field.FieldType), field.Name)));
		}

		private static string GetArrayDecl (MemberInfo member, ref Type memberType)
		{
			var arrayDecl = "";
			if (memberType.IsArray)
			{
				var arrAttr = member.ExpectFixedArrayAttribute ();
				arrayDecl = string.Format ("[{0}]", arrAttr.Length);
				memberType = memberType.GetElementType ();
			}
			return arrayDecl;
		}

		private void DeclareUniforms (Type type)
        {
			if (!DefineType (type)) return;
			foreach (var field in type.GetUniforms ())
			{
				var uniType = field.FieldType.GetGenericArguments ().Single ();
				string arrayDecl = GetArrayDecl (field, ref uniType);
				if (uniType.GetGLAttribute () is GLStruct)
					DefineStruct (uniType);
				DeclOut ("uniform {0} {1}{2};", MapType (uniType), field.Name, arrayDecl);
			}
		}

		private void DeclareVariable (MemberInfo member, Type memberType, string prefix)
        {
            if (!(member.IsBuiltin () || member.IsDefined (typeof (OmitInGlslAttribute), true) ||
				member.Name.StartsWith ("<>")))
            {
				string arrayDecl = GetArrayDecl (member, ref memberType);
				var syntax = MapType (memberType);
				var qualifiers = member.GetQualifiers ();
				DeclOut (string.IsNullOrEmpty (qualifiers) ?
                    string.Format ("{0} {1} {2}{3};", prefix, syntax, member.Name, arrayDecl) :
                    string.Format ("{0} {1} {2} {3}{4};", qualifiers, prefix, syntax, member.Name, arrayDecl));
            }
        }

        private void DeclareVariables (Type type, string prefix, string instanceName)
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
				var elemGLType = MapType (elemType);
				var nai = value.Expect<NewArrayExpression> (ExpressionType.NewArrayInit);
				CodeOut ("const {0} {1}[{2}] = {0}[] (\n\t{3});",
					elemGLType, name, nai.Expressions.Count,
					nai.Expressions.Select (Expr).SeparateWith (",\n\t"));
			}
			else
				CodeOut ("const {0} {1} = {2};", MapType (constType), name, Expr (value));
		}

		protected override void OutputFromBinding (ParameterExpression par, MethodCallExpression node)
		{
			if (node.Method.Name == "State")
				return;
			var type = node.Method.GetGenericArguments () [0];
			if (node.Method.Name == "Inputs")
				DeclareVariables (type, "in", par.Name);
			else if (node.Method.Name == "Uniforms")
				DeclareUniforms (type);
			else if (node.Method.Name == "Constants")
				DeclareConstants (node.Arguments [0]);
			else if (node.Method.Name == "ToShader")
				CodeOut ("{0} {1} = {2};", MapType (type), par.Name, Expr (node.Arguments [0]));
			else
				throw new ArgumentException ("Unsupported lift method.", node.Method.ToString ());
		}

		private void ReturnArrayOfVertices (Expression expr)
		{
			var nai = expr.Expect<NewArrayExpression> (ExpressionType.NewArrayInit);
			foreach (var subExpr in nai.Expressions)
			{
				var mie = subExpr.Expect<MemberInitExpression> (ExpressionType.MemberInit);
				foreach (MemberAssignment assign in mie.Bindings)
					CodeOut ("{0} = {1};", assign.Member.Name, Expr (assign.Expression));
				CodeOut ("EmitVertex ();");
			}
			CodeOut ("EndPrimitive ();");
		}

		private void OutputShader (LambdaExpression expr)
        {
			StartMain ();
			var retExpr = ParseLinqExpression (expr.Body);
            ConditionalReturn (retExpr, Return);
			EndFunction ();
        }

		private void OutputGeometryShader (LambdaExpression expr)
		{
			StartMain ();
			var retExpr = ParseLinqExpression (expr.Body);
			ConditionalReturn (retExpr, ReturnArrayOfVertices);
			EndFunction ();
		}
	}
}
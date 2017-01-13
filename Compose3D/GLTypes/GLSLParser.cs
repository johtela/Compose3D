namespace Compose3D.GLTypes
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

	public class GlslParser : LinqParser
    {
		private GlslParser () : base (typeof (Shader), new GLTypeMapping ())
        { }

		public static string CreateShader<T> (string version, Expression<Func<Shader<T>>> shader)
		{
			var parser = new GlslParser ();
			parser.DeclareVaryings (typeof (T), GlslAst.VaryingKind.Out);
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
			var parser = new GlslParser ();
			parser._program = Ast.Prog ();
			if (invocations > 0)
				parser.DeclOut ("layout (invocations = {0}) in;", invocations);
			parser.DeclOut ("layout ({0}) in;", inputPrimitive.MapInputGSPrimitive ());
			parser.DeclOut ("layout ({0}, max_vertices = {1}) out;", 
				outputPrimitive.MapOutputGSPrimitive (), vertexCount);
			parser.DeclareVaryings (typeof (T), GlslAst.VaryingKind.Out);
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
			CreateFunction (new GlslParser (), name, expr);
		}

		private string BuildShaderCode ()
		{
			AstTransform.IncludeCalledFunctions (_function, _program);
			_program.Globals.Add (_function);
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
			var glfield = me.Member.GetAttribute<GLFieldAttribute> ();
			if (glfield != null)
				return Ast.FRef (Expr (me.Expression), Ast.Fld (glfield.Name));
			var syntax = me.Member.GetGLSyntax ();
			if (syntax != null)
				return Ast.Call (syntax, Expr (me.Expression));
			if (me.Member.IsBuiltin ())
				return Ast.VRef (Ast.Var (MapType (me.Type), me.Member.Name));
			var declType = me.Expression.Type;
			if (declType.IsGLStruct () && _typesDefined.Contains (declType))
			{
				var field = _program != null ?
					_program.FindStruct (declType.Name).FindField (me.Member.Name) :
					Ast.Fld (me.Member.Name);
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

		private static int GetArrayLen (MemberInfo member, ref Type memberType)
		{
			if (memberType.IsArray)
			{
				memberType = memberType.GetElementType ();
				return member.ExpectFixedArrayAttribute ().Length;
			}
			return 0;
		}

		private void DeclareUniforms (Type type)
        {
			if (!DefineType (type)) return;
			foreach (var field in type.GetUniforms ())
			{
				var uniType = field.FieldType.GetGenericArguments ().Single ();
				var arrayLen = GetArrayLen (field, ref uniType);
				if (uniType.GetGLAttribute () is GLStruct)
					DefineStruct (uniType);
				var unif = GlslAst.Unif (MapType (uniType), field.Name, arrayLen);
				if (_program != null)
					_program.Globals.Add (unif);
				_globals.Add (field.Name, unif.Definition);
			}
		}

		private void DeclareVarying (MemberInfo member, Type memberType, GlslAst.VaryingKind kind)
        {
            if (!member.Name.StartsWith ("<>"))
            {
				var arrayLen = GetArrayLen (member, ref memberType);
				var type = MapType (memberType);
				var qualifiers = member.GetQualifiers ();
				var vary = GlslAst.Vary (kind, qualifiers, type, member.Name, arrayLen);
				if (!(member.IsBuiltin () || member.IsDefined (typeof (OmitInGlslAttribute), true)))
					_program.Globals.Add (vary);
				_globals.Add (member.Name, vary.Definition);
			}
		}

		private void DeclareVaryings (Type type, GlslAst.VaryingKind kind)
        {
			if (!DefineType (type))
				return;
            if (!type.Name.StartsWith ("<>"))
				foreach (var field in type.GetGLFields ())
					DeclareVarying (field, field.FieldType, kind);
			foreach (var prop in type.GetGLProperties ())
				DeclareVarying (prop, prop.PropertyType, kind);
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
				var elemType = MapType (constType.GetElementType ());
				var nai = value.Expect<NewArrayExpression> (ExpressionType.NewArrayInit);
				con = Ast.Const (elemType, name, nai.Expressions.Count, Expr (value));
			}
			else
				con = Ast.Const (MapType (constType), name, Expr (value));
			CodeOut (GlslAst.DeclConst (con));
			_constants.Add (name, con);
		}

		protected override void OutputFromBinding (ParameterExpression par, MethodCallExpression node)
		{
			if (node.Method.Name == "State")
				return;
			var type = node.Method.GetGenericArguments () [0];
			if (node.Method.Name == "Inputs")
				DeclareVaryings (type, GlslAst.VaryingKind.In);
			else if (node.Method.Name == "Uniforms")
				DeclareUniforms (type);
			else if (node.Method.Name == "Constants")
				DeclareConstants (node.Arguments[0]);
			else if (node.Method.Name == "ToShader")
				DeclareLocal (MapType (type), par.Name, Expr (node.Arguments[0]));
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
					CodeOut (Ast.Ass (Ast.VRef (_globals [assign.Member.Name]), Expr (assign.Expression)));
				CodeOut (Ast.CallS (Ast.Call ("EmitVertex ()")));
			}
			CodeOut (Ast.CallS (Ast.Call ("EndPrimitive ()")));
		}

		private void StartMain ()
		{
			_function = Ast.Fun ("main", "void", Enumerable.Empty<Ast.Argument> (), Ast.Blk ());
			StartScope (_function.Body);
		}

		private void OutputShader (LambdaExpression expr)
        {
			StartMain ();
			var retExpr = ParseLinqExpression (expr.Body);
            ConditionalReturn (retExpr, Return);
			EndScope ();
		}

		private void OutputGeometryShader (LambdaExpression expr)
		{
			StartMain ();
			var retExpr = ParseLinqExpression (expr.Body);
			ConditionalReturn (retExpr, ReturnArrayOfVertices);
			EndScope ();
		}
	}
}
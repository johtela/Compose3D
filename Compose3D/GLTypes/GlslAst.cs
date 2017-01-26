namespace Compose3D.GLTypes
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Extensions;
	using Compiler;

	public class GlslAst
	{
		public class Uniform : Ast.Global
		{
			public readonly Variable Definition;

			internal Uniform (Type type, string name, int arraylen)
				: base (name)
			{
				Definition = new Ast.Variable (type, name, arraylen);
			}

			public override string Output (LinqParser parser)
			{
				return string.Format ("uniform {0};", Definition.Output (parser));
			}
		}

		public enum VaryingKind { In, Out }

		public class Varying : Ast.Global
		{
			public readonly VaryingKind Kind;
			public readonly string Qualifiers;
			public readonly Variable Definition;

			internal Varying (VaryingKind kind, string qualifiers, Type type, string name, 
				int arraylen) : base (name)
			{
				Kind = kind;
				Qualifiers = qualifiers;
				Definition = new Ast.Variable (type, name, arraylen);
			}

			public override string Output (LinqParser parser)
			{
				var kindDecl = Kind == VaryingKind.In ? "in" : "out";
				return string.IsNullOrEmpty (Qualifiers) ?
					string.Format ("{0} {1};", kindDecl, Definition.Output (parser)) :
					string.Format ("{0} {1} {2};", Qualifiers, kindDecl, Definition.Output (parser));
			}
		}

		public class Structure : Ast.Structure
		{
			public Structure (string name, IEnumerable<Field> fields)
				: base (name,fields) { }

			public override string Output (LinqParser parser)
			{
				return string.Format ("struct {0}\n{{\n{1}\n}};", Name,
					Fields.Select (f => string.Format ("    {0};", f.Output (parser)))
					.SeparateWith ("\n"));
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var fields = Fields.Select (f => (Field)f.Transform (transform));
				return transform (fields.SequenceEqual (Fields) ? this :
					new GlslAst.Structure (Name, fields));
			}
		}

		public class DeclareConstant : Ast.Statement
		{
			public readonly Constant Definition;

			internal DeclareConstant (Constant definition)
			{
				Definition = definition;
			}

			public override string Output (LinqParser parser)
			{
				return Tabs () + Definition.Output (parser) + ";\n";
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var def = (Constant)transform (Definition);
				return transform  (def == Definition ? this :
					new DeclareConstant (def));
			}
		}

		public static Uniform Unif (Type type, string name, int arraylen)
		{
			return new Uniform (type, name, arraylen);
		}

		public static Varying Vary (VaryingKind kind, string qualifiers, Type type, 
			string name, int arraylen)
		{
			return new Varying (kind, qualifiers, type, name, arraylen);
		}

		public static Structure Struct (string name, IEnumerable<Ast.Field> fields)
		{
			return new Structure (name, fields);
		}

		public static DeclareConstant DeclConst (Ast.Constant definition)
		{
			return new DeclareConstant (definition);
		}
	}
}

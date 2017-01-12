namespace Compose3D.GLTypes
{
	using System;
	using Compiler;

	public class GlslAst
	{
		public class Uniform : Ast.Global
		{
			public readonly Variable Definition;

			internal Uniform (string type, string name, int arraylen)
			{
				Definition = new Ast.Variable (type, name, arraylen);
			}

			public override string ToString ()
			{
				return string.Format ("uniform {0};", Definition);
			}
		}

		public enum VaryingKind { In, Out }

		public class Varying : Ast.Global
		{
			public readonly VaryingKind Kind;
			public readonly string Qualifiers;
			public readonly Variable Definition;

			internal Varying (VaryingKind kind, string qualifiers, string type, string name, 
				int arraylen)
			{
				Kind = kind;
				Qualifiers = qualifiers;
				Definition = new Ast.Variable (type, name, arraylen);
			}

			public override string ToString ()
			{
				var kindDecl = Kind == VaryingKind.In ? "in" : "out";
				return string.IsNullOrEmpty (Qualifiers) ?
					string.Format ("{0} {1} {2};", Qualifiers, kindDecl, Definition) :
					string.Format ("{0} {1};", kindDecl, Definition);
			}
		}

		public class DeclareConstant : Ast.Statement
		{
			public readonly Constant Definition;

			internal DeclareConstant (Constant definition)
			{
				Definition = definition;
			}

			public override string ToString ()
			{
				return Definition.ToString () + ";";
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var def = (Constant)transform (Definition);
				return transform  (def == Definition ? this :
					new DeclareConstant (def));
			}
		}

		public static Uniform Unif (string type, string name, int arraylen)
		{
			return new Uniform (type, name, arraylen);
		}

		public static Varying Vary (VaryingKind kind, string qualifiers, string type, 
			string name, int arraylen)
		{
			return new Varying (kind, qualifiers, type, name, arraylen);
		}

		public static DeclareConstant DeclConst (Ast.Constant definition)
		{
			return new DeclareConstant (definition);
		}
	}
}

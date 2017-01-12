namespace Compose3D.GLTypes
{
	using System;
	using Compiler;

	public class GlslAst
	{
		public class Uniform : Ast.Global
		{
			public readonly string Type;
			public readonly string Name;
			public readonly int ArrayLength;

			internal Uniform (string type, string name, int arraylen)
			{
				Type = type;
				Name = name;
				ArrayLength = arraylen;
			}

			public override string ToString ()
			{
				return ArrayLength > 0 ?
					string.Format ("uniform {0} {1}[{2}];", Type, Name, ArrayLength) :
					string.Format ("uniform {0} {1};", Type, Name);
			}
		}

		public enum VaryingKind { In, Out }

		public class Varying : Ast.Global
		{
			public readonly VaryingKind Kind;
			public readonly string Qualifier;
			public readonly string Type;
			public readonly string Name;
			public readonly int ArrayLength;

			internal Varying (VaryingKind kind, string qualifier, string type, string name, 
				int arraylen)
			{
				Kind = kind;
				Qualifier = qualifier;
				Type = type;
				Name = name;
				ArrayLength = arraylen;
			}

			public override string ToString ()
			{
				var kindDecl = Kind == VaryingKind.In ? "in" : "out";
				var arrayDecl = ArrayLength > 0 ?
					string.Format ("[{0}]", ArrayLength) : "";
				return Qualifier != null ?
					string.Format ("{0} {1} {2} {3}{4};", Qualifier, kindDecl, Type, Name, arrayDecl) :
					string.Format ("{0} {1} {2}{3};", kindDecl, Type, Name, arrayDecl);
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
	}
}

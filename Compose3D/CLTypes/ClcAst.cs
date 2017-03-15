namespace Compose3D.CLTypes
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Extensions;
	using Compiler;

	public class ClcAst
	{
		public enum KernelArgumentKind { Value, Buffer, Image }
		public enum KernelArgumentAccess { ReadWrite, Read, Write }
		public enum KernelArgumentMemory { Default, Global, Local }

		public class KernelArgument : Ast.Argument
		{
			public readonly KernelArgumentKind Kind;
			public readonly KernelArgumentAccess Access;
			public readonly KernelArgumentMemory Memory;

			internal KernelArgument (Type type, string name, KernelArgumentKind kind,
				KernelArgumentMemory memory, KernelArgumentAccess access)
				: base (type, name, 0)
			{
				Kind = kind;
				Access = access;
				Memory = memory;
			}

			public override string Output (LinqParser parser)
			{
				var result = string.Format ("{0}{1} {2}", parser.MapType (Type), 
					Kind == KernelArgumentKind.Buffer ? "*" : "",
					Name);
				switch (Access)
				{
					case KernelArgumentAccess.Read:
						result = "read_only " + result;
						break;
					case KernelArgumentAccess.Write:
						result = "write_only " + result;
						break;
					default:
						break;
				}
				switch (Memory)
				{
					case KernelArgumentMemory.Global:
						result = "global " + result;
						break;
					case KernelArgumentMemory.Local:
						result = "local " + result;
						break;
					default:
						break;
				}
				return result;
			}
		}

		public class Kernel : Ast.Function
		{
			internal Kernel (string name, IEnumerable<KernelArgument> arguments, Block body)
				: base (name, null, arguments, body) { }

			public override string Output (LinqParser parser)
			{
				return string.Format ("kernel void {0} ({1})\n{2}", Name,
					Arguments.Select (v => v.Output (parser)).SeparateWith (", "),
					Body.Output (parser));
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var args = Arguments.Select (a => (KernelArgument)a.Transform (transform));
				var body = (Block)Body.Transform (transform);
				return transform (args.SequenceEqual (Arguments) && body == Body ? this :
					new Kernel (Name, args, body));
			}
		}

		public class Structure : Ast.Structure
		{
			public readonly bool IsUnion;

			public Structure (string name, IEnumerable<Field> fields, bool isUnion)
				: base (name, fields)
			{
				IsUnion = isUnion;
			}

			public override string Output (LinqParser parser)
			{
				return string.Format ("typedef {0} __attribute__ ((packed)) tag_{1}\n{{\n{2}\n}} {1};\n", 
					IsUnion ? "union" : "struct",
					Name,
					Fields.Select (f => string.Format ("    {0};", f.Output (parser)))
					.SeparateWith ("\n"));
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var fields = Fields.Select (f => (Field)f.Transform (transform));
				return transform (fields.SequenceEqual (Fields) ? this :
					new ClcAst.Structure (Name, fields, IsUnion));
			}
		}

		public class ClcNewArray : Ast.NewArray
		{
			internal ClcNewArray (Type type, int count, IEnumerable<Expression> items)
				: base (type, count, items) { }

			public override string Output (LinqParser parser)
			{
				return string.Format ("{{ {0} }}", 
					Items.Select (e => e.Output (parser)).SeparateWith (", "));
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var items = Items.Select (i => (Expression)i.Transform (transform));
				return transform (items.SequenceEqual (Items) ? this :
					new ClcNewArray (ItemType, ItemCount, items));
			}
		}

		public class ClcInitStruct : Ast.InitStruct
		{
			internal ClcInitStruct (Type structType, IEnumerable<Tuple<VariableRef, Expression>> initFields)
				: base (structType, initFields) { }

			public override string Output (LinqParser parser)
			{
				return string.Format ("({0}) {{ {1} }}", 
					parser.MapType (StructType),
					InitFields.Select (t => (StructType.IsCLUnion () ? 
						string.Format (".{0} = {1}", t.Item1.Output (parser), t.Item2.Output (parser)) :
						t.Item2.Output (parser)))
					.SeparateWith (", "));
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var ifs = InitFields.Select (t => Tuple.Create (
					(VariableRef)t.Item1.Transform (transform),  
					(Expression)t.Item2.Transform (transform)));
				return transform (ifs.SequenceEqual (InitFields) ? this :
					new ClcInitStruct (StructType, ifs));
			}
		}

		public class DeclareConstant : Ast.Global
		{
			public readonly Constant Definition;

			internal DeclareConstant (Constant definition)
				: base (definition.Name)
			{
				Definition = definition;
			}

			public override string Output (LinqParser parser)
			{
				return "constant " + Definition.Output (parser) + ";\n";
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var def = (Constant)transform (Definition);
				return transform (def == Definition ? this :
					new DeclareConstant (def));
			}
		}

		public static KernelArgument KArg (Type type, string name, KernelArgumentKind kind,
			KernelArgumentMemory memory, KernelArgumentAccess access)
		{
			return new KernelArgument (type, name, kind, memory, access);
		}

		public static KernelArgument KArg (Type type, string name, KernelArgumentKind kind,
			KernelArgumentMemory memory)
		{
			return new KernelArgument (type, name, kind, memory, KernelArgumentAccess.ReadWrite);
		}

		public static KernelArgument KArg (Type type, string name, KernelArgumentKind kind)
		{
			return new KernelArgument (type, name, kind, KernelArgumentMemory.Default, 
				KernelArgumentAccess.ReadWrite);
		}

		public static Kernel Kern (string name, IEnumerable<KernelArgument> arguments,
			Ast.Block body)
		{
			return new Kernel (name, arguments, body);
		}

		public static Structure Struct (string name, IEnumerable<Ast.Field> fields)
		{
			return new Structure (name, fields, false);
		}

		public static Structure Union (string name, IEnumerable<Ast.Field> fields)
		{
			return new Structure (name, fields, true);
		}

		public static ClcNewArray Arr (Type type, int count, IEnumerable<Ast.Expression> items)
		{
			return new ClcNewArray (type, count, items);
		}

		public static ClcInitStruct InitS (Type structType, 
			IEnumerable<Tuple<Ast.VariableRef, Ast.Expression>> initFields)
		{
			return new ClcInitStruct (structType, initFields);
		}

		public static DeclareConstant DeclConst (Ast.Constant definition)
		{
			return new DeclareConstant (definition);
		}
	}
}

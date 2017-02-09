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

			public KernelArgument (Type type, string name, KernelArgumentKind kind,
				KernelArgumentAccess access, KernelArgumentMemory memory)
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
			public Kernel (string name, IEnumerable<KernelArgument> arguments, Block body)
				: base (name, null, arguments, body) { }

			public override string Output (LinqParser parser)
			{
				return string.Format ("kernel void {0} ({1})\n{2}", Name,
					Arguments.Select (v => v.Output (parser)).SeparateWith (", "),
					Body.Output (parser));
			}
		}

		public static KernelArgument KArg (Type type, string name, KernelArgumentKind kind,
			KernelArgumentAccess access, KernelArgumentMemory memory)
		{
			return new KernelArgument (type, name, kind, access, memory);
		}

		public static Kernel Kern (string name, IEnumerable<KernelArgument> arguments,
			Ast.Block body)
		{
			return new Kernel (name, arguments, body);
		}
	}
}

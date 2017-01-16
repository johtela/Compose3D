namespace Compose3D.Compiler
{
	using System;
	using System.Collections.Generic;

	internal class CompiledFunction
	{
		public readonly Ast.Program Program;
		public readonly Ast.Function Function;
		public readonly HashSet<Type> TypesDefined;

		public CompiledFunction (Ast.Program program, Ast.Function function,
			HashSet<Type> typesDefined)
		{
			Program = program;
			Function = function;
			TypesDefined = typesDefined;
		}
	}
}

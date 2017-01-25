namespace Compose3D.Compiler
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;

	internal class CompiledFunction
	{
		public readonly Ast.Program Program;
		public readonly Ast.Function Function;
		public readonly HashSet<Type> TypesDefined;

		private static Dictionary<MemberInfo, CompiledFunction> _functions =
			new Dictionary<MemberInfo, CompiledFunction> ();

		public CompiledFunction (Ast.Program program, Ast.Function function,
			HashSet<Type> typesDefined)
		{
			Program = program;
			Function = function;
			TypesDefined = typesDefined;
		}

		public static void Add (MemberInfo member, CompiledFunction function)
		{
			_functions.Add (member, function);
		}

		public static CompiledFunction Get (MemberInfo member)
		{
			CompiledFunction result;
			return _functions.TryGetValue (member, out result) ? result : null;
		}
	}
}

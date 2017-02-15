namespace Compose3D.Compiler
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;

	internal class Function
	{
		public readonly Ast.Program Program;
		public readonly Ast.Function AstFunction;
		public readonly HashSet<Type> TypesDefined;

		private static Dictionary<MemberInfo, Function> _functions =
			new Dictionary<MemberInfo, Function> ();

		public Function (Ast.Program program, Ast.Function function,
			HashSet<Type> typesDefined)
		{
			Program = program;
			AstFunction = function;
			TypesDefined = typesDefined;
		}

		public static void Add (MemberInfo member, Function function)
		{
			_functions.Add (member, function);
		}

		public static Function Get (MemberInfo member)
		{
			Function result;
			return _functions.TryGetValue (member, out result) ? result : null;
		}
	}
}

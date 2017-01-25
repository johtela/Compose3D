namespace Compose3D.Compiler
{
	using System;
	using System.Linq;
	using Extensions;

	public delegate TRes Macro<TRes> ();
	public delegate TRes Macro<T1, TRes> (T1 arg1);
	public delegate TRes Macro<T1, T2, TRes> (T1 arg1, T2 arg2);
	public delegate TRes Macro<T1, T2, T3, TRes> (T1 arg1, T2 arg2, T3 arg3);
	public delegate TRes Macro<T1, T2, T3, T4, TRes> (T1 arg1, T2 arg2, T3 arg3, T4 arg4);
	public delegate TRes Macro<T1, T2, T3, T4, T5, TRes> (T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
	public delegate TRes Macro<T1, T2, T3, T4, T5, T6, TRes> (T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);

	public static class Macro
	{
		public static bool IsMacroType (this Type type)
		{
			return type.IsGenericType && type.GetGenericTypeDefinition ().In (
				typeof (Macro<>), typeof (Macro<,>), typeof (Macro<,,>), typeof (Macro<,,,>), 
				typeof (Macro<,,,,>), typeof (Macro<,,,,,>), typeof (Macro<,,,,,,>));
		}

		public static Ast.MacroDefinition GetMacroDefinition (this Type type, Func<Type, string> mapType)
		{
			if (!type.IsMacroType ())
				throw new ArgumentException ("Given type is not a macro type.", nameof (type));
			var gtypes = type.GetGenericArguments ();
			var argLen = gtypes.Length - 1;
			var res = Ast.MRes (mapType (gtypes[argLen]));
			var pars = from t in gtypes.Take (argLen)
					   select t.IsMacroType () ?
							Ast.MDPar (GetMacroDefinition (t, mapType)) :
							Ast.MPar (mapType (t));
			return Ast.MDef (pars, res);
		}

		internal static Ast.Function InstantiateAllMacros (Ast.Function function)
		{
			while (true)
			{
				var instantiated = (Ast.Function)function.Transform (node =>
					node is Ast.MacroCall ? InstantiateMacro (node as Ast.MacroCall) : node);
				if (instantiated == function)
					return function;
				function = instantiated;
			}
		}

		internal static Ast.Block InstantiateMacro (Ast.MacroCall call)
		{
			var macro = call.Target as Ast.Macro;
			if (macro == null)
				throw new InvalidOperationException ("Trying to instantiate macro definition.");
			if (call.Parameters.Any (mp => mp.GetType () == typeof (Ast.MacroDefinition)))
				throw new InvalidOperationException ("Uninstantiated macro parameter in macro call.");
			return (Ast.Block)macro.Implementation.Transform (node =>
			{
				if (node is Ast.MacroDefinition)
				{
					if (node is Ast.Macro)
						return node;
					var macroDef = node as Ast.MacroDefinition;
					var i = macro.GetMacroDefParamIndex (macroDef);
					if (i >= 0)
						return call.Parameters[i];
				}
				if (node is Ast.MacroParamRef)
				{
					var mp = node as Ast.MacroParamRef;
					var i = macro.GetParamRefIndex (mp);
					if (i >= 0)
						return call.Parameters[i];
				}
				if (node is Ast.MacroResultVar && node == macro.Result)
					return call.ResultVar;
				return node;
			});
		}
	}
}

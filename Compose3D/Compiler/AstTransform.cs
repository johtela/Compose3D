namespace Compose3D.Compiler
{
	using System;
	using System.Linq;
	using Extensions;

	internal static class AstTransform
	{
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
			if (call.Parameters.Any (mp => mp.GetType () ==  typeof (Ast.MacroDefinition)))
				throw new InvalidOperationException ("Uninstantiated macro parameter in macro call.");
			return (Ast.Block)macro.Implementation.Transform (node =>
			{
				if (node is Ast.MacroDefinition)
				{
					if (node is Ast.Macro)
						return node;
					var macroDef = node as Ast.MacroDefinition;
					var i = macro.GetMacroDefParamIndex (macroDef);
					if (i < 0)
						throw new InvalidOperationException ("Macro reference not in scope.");
					return call.Parameters[i];
				}
				if (node is Ast.MacroParamRef)
				{
					var mp = node as Ast.MacroParamRef;
					var i = macro.GetParamRefIndex (mp);
					if (i < 0)
						throw new InvalidOperationException ("Macro expression parameter not in scope.");
					return call.Parameters[i];
				}
				if (node is Ast.MacroResultVar && node == macro.Result)
					return call.ResultVar;
				return node;
			});
		}
	}
}

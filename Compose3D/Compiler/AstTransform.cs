namespace Compose3D.Compiler
{
	using System;
	using System.Linq;
	using Extensions;

	internal static class AstTransform
	{
		internal static Ast.Function IncludeCalledFunctions (Ast.Function function, Ast.Program program)
		{
			if (function.IsHigherOrder)
				throw new ArgumentException ("Cannot expand higher order function. It must be instantiated first.",
					nameof (function));
			return (Ast.Function)function.Transform (node =>
			{
				if (node is Ast.FunctionCall)
				{
					var call = node as Ast.FunctionCall;
					Ast.Function func;
					if (node is Ast.HigherOrderFunctionCall)
					{
						func = InstantiateHigherOrderFunction (node as Ast.HigherOrderFunctionCall);
						call = new Ast.FunctionCall (new Ast.NamedFunctionRef (func), call.Arguments);
					}
					else
					{
						func = ((Ast.NamedFunctionRef)call.FuncRef).Target;
						if (func.IsExternal || program.DefinedFunctions.Contains (func))
							return call;
					}
					IncludeCalledFunctions (func, program);
					program.Globals.Add (func);
					return call;
				}
				return node;
			});
		}

		internal static Ast.Function InstantiateHigherOrderFunction (Ast.HigherOrderFunctionCall call)
		{
			var func = call.NamedFuncRef.Target;
			var body = (Ast.Block)func.Body.Transform (node =>
			{
				if (node is Ast.FunctionArgumentRef)
				{
					var funcarg = (node as Ast.FunctionArgumentRef).Target;
					var i = func.FunctionArguments.IndexOf (funcarg);
					if (i < 0)
						throw new InvalidOperationException ("Function argument reference not in scope.");
					return call.FunctionArguments[i];
				}
				return node;
			});
			return new Ast.Function (func.Name + call.GetHashCode ().ToString ("x"), func.ReturnType,
				func.Arguments, Enumerable.Empty<Ast.FunctionArgument> (), body);
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
			if (!call.MacroParameters.All (mp => mp is Ast.Macro))
				throw new InvalidOperationException ("Uninstantiated macro parameter in macro call.");
			return (Ast.Block)macro.Implementation.Transform (node =>
			{
				if (node is Ast.MacroDefinition)
				{
					if (node is Ast.Macro)
						return node;
					var macroDef = node as Ast.MacroDefinition;
					var i = call.MacroParameters.IndexOf (macroDef);
					if (i < 0)
						throw new InvalidOperationException ("Macro reference not in scope.");
					return call.MacroParameters[i];
				}
				if (node is Ast.MacroParamRef)
				{
					var mp = node as Ast.MacroParamRef;
					var i = call.Parameters.IndexOf (mp);
					if (i < 0)
						throw new InvalidOperationException ("Macro expression parameter not in scope.");
					return call.Parameters[i];
				}
				return node;
			});
		}
	}
}

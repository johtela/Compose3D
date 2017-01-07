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
	}
}

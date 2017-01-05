namespace Compose3D.Compiler
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	internal static class AstTransform
	{
		internal static Ast.Function IncludeCalledFunctions (Ast.Function function, Ast.Program program)
		{
			return (Ast.Function)function.Transform (node =>
			{
				if (node is Ast.Call)
				{
					var call = node as Ast.Call;
					var func = call.FuncRef.Target;
					if (!func.IsHigherOrder)
					{
						if (!(func.IsExternal || program.DefinedFunctions.Contains (func)))
						{
							IncludeCalledFunctions (func, program);
							program.Globals.Add (func);
						}
					}
					else
					{

					}
				}
				return node;
			});
		}

		internal static Ast.Call InstantiateHigherOrderFunction (Ast.Call call, 
			Ast.Program program)
		{
			var func = call.FuncRef.Target;
			var body = (Ast.Block)func.Body.Transform (node =>
			{
				if (node is Ast.VariableRef)
				{
					var variable = (node as Ast.VariableRef).Target;
					if (variable is Ast.FunctionArgument)
					{
						var funArg = variable as Ast.FunctionArgument;
						
					}
				}
				return node;
			});
			return call;
		}
	}
}

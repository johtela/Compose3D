namespace Compose3D.CLTypes
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Extensions;
	using Compiler;

	public class ClcAst 
	{
		public class Kernel : Ast.Function
		{
			public Kernel (string name, IEnumerable<Argument> arguments, Block body)
				: base (name, null, arguments, body)
			{
			}


		}
	}
}

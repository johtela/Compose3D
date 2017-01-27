namespace Compose3D.Compiler
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Extensions;

	public static class Aggregate<T>
	{
		private static Ast.Macro ForMacro ()
		{
			var def = typeof (Macro<int, int, int, T, Macro<int, T, T>, T>).GetMacroDefinition ();
			var acc = Macro.GenUniqueVar (typeof (T), "acc");
			var ind = Macro.GenUniqueVar (typeof (int), "ind");
			var init = Ast.MPRef (def.Parameters[0]);
			var cond = Ast.Op ("{0} != {1}", Ast.VRef (ind), Ast.MPRef (def.Parameters[1]));
			var incr = Ast.Op ("{0} += {1}", Ast.VRef (ind), Ast.MPRef (def.Parameters[2]));
			var body = (def.Parameters[4] as Ast.MacroDefParam).Definition;
			return new Ast.Macro (def.Parameters, def.Result,
				Ast.Blk	(
					Ast.DeclVar (acc, Ast.MPRef (def.Parameters[3])),
					Ast.For (ind, init, cond, incr, 
						Ast.MCall (body, acc, Ast.VRef (ind), Ast.VRef (acc))
					),
					Ast.Ass (Ast.VRef (def.Result), Ast.VRef (acc))
				)
			);
		}

		public static readonly Macro<int, int, int, T, Macro<int, T, T>, T> For =
			Macro.Create (() => For, ForMacro ());
	}
}

namespace Compose3D.Compiler
{
	public static class Array<T>
	{
		private static Ast.Macro ChangeMacro ()
		{
			var def = typeof (Macro<T[], int, T, T[]>).GetMacroDefinition ();
			var arr = Ast.MPRef (def.Parameters[0]);
			var ind = Ast.MPRef (def.Parameters[1]);
			var val = Ast.MPRef (def.Parameters[2]);
			return Ast.Mac (def.Parameters, def.Result, Ast.Blk (
				Ast.Ass (Ast.VRef (def.Result), arr),
				Ast.Ass (Ast.ARef (def.Result, ind), val)));
		}

		public static readonly Macro<T[], int, T, T[]> Change =
			Macro.Create (() => Change, ChangeMacro ());
	}
}

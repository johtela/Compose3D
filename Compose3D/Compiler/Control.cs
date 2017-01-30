namespace Compose3D.Compiler
{
	public static class Control<T>
	{
		private static Ast.Macro ForStepMacro ()
		{
			var def = typeof (Macro<int, int, int, T, Macro<int, T, T>, T>).GetMacroDefinition ();
			var ind = Macro.GenUniqueVar (typeof (int), "ind");
			var init = Ast.MPRef (def.Parameters[0]);
			var cond = Ast.Op ("{0} != {1}", Ast.VRef (ind), Ast.MPRef (def.Parameters[1]));
			var incr = Ast.Op ("{0} += {1}", Ast.VRef (ind), Ast.MPRef (def.Parameters[2]));
			var body = (def.Parameters[4] as Ast.MacroDefParam).Definition;
			return Ast.Mac (def.Parameters, def.Result, Ast.Blk	(
				Ast.Ass (Ast.VRef (def.Result), Ast.MPRef (def.Parameters[3])),
				Ast.For (ind, init, cond, incr, 
					Ast.MCall (body, def.Result, Ast.VRef (ind), Ast.VRef (def.Result)))));
		}

		private static Ast.Macro ForMacro ()
		{
			var def = typeof (Macro<int, int, T, Macro<int, T, T>, T>).GetMacroDefinition ();
			var ind = Macro.GenUniqueVar (typeof (int), "ind");
			var init = Ast.MPRef (def.Parameters[0]);
			var cond = Ast.Op ("{0} < {1}", Ast.VRef (ind), Ast.MPRef (def.Parameters[1]));
			var incr = Ast.Op ("{0}++", Ast.VRef (ind));
			var body = (def.Parameters[3] as Ast.MacroDefParam).Definition;
			return Ast.Mac (def.Parameters, def.Result,	Ast.Blk (
				Ast.Ass (Ast.VRef (def.Result), Ast.MPRef (def.Parameters[2])),
				Ast.For (ind, init, cond, incr,
					Ast.MCall (body, def.Result, Ast.VRef (ind), Ast.VRef (def.Result)))));
		}

		private static Ast.Macro IfMacro ()
		{
			var def = typeof (Macro<bool, Macro<T>, Macro<T>, T>).GetMacroDefinition ();
			var cond = Ast.MPRef (def.Parameters[0]);
			var thenm = (def.Parameters[1] as Ast.MacroDefParam).Definition;
			var elsem = (def.Parameters[2] as Ast.MacroDefParam).Definition;
			return Ast.Mac (def.Parameters, def.Result, Ast.Blk (
				Ast.If (cond, Ast.MCall (thenm, def.Result), Ast.MCall (elsem, def.Result))));
		}

		public static readonly Macro<int, int, int, T, Macro<int, T, T>, T> ForStep =
			Macro.Create (() => ForStep, ForStepMacro ());

		public static readonly Macro<int, int, T, Macro<int, T, T>, T> For =
			Macro.Create (() => For, ForMacro ());

		public static readonly Macro<bool, Macro<T>, Macro<T>, T> If =
			Macro.Create (() => If, IfMacro ());
	}
}

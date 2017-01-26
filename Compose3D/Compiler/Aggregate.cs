namespace Compose3D.Compiler
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	public static class Aggregate<T>
	{
		public static readonly Macro<int, int, int, Macro<int, T, T>, T> For =
			Macro.Create<int, int, int, Macro<int, T, T>, T>
			(
				() => For,
				Ast.Mac (Enumerable.Empty<Ast.MacroParam> (), Ast.MRes (typeof (T).Name), Ast.Blk ())
			);
	}
}

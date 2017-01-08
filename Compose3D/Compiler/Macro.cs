namespace Compose3D.Compiler
{
	using System;
	using System.Collections.Generic;
	using System.Linq.Expressions;
	using System.Text;
	using System.Threading.Tasks;

	public delegate U Macro<T, U> (T arg1);

	public static class Macro
	{
		public static void Foo (Macro<int, int> macro)
		{
			var i = macro (3);
		}

		public static void Bar ()
		{
			Foo (i => 2);
		}

		public static void Baz ()
		{
			var expr = Expression.Lambda<Macro<int, int>> (Expression.Constant (1), Expression.Parameter (typeof (int)));
			Foo (expr.Compile ());
		}
	}
}

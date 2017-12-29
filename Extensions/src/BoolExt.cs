/*
# Extensions for Boolean Type

The boolean type currently has only one extension method.
*/
namespace Extensions
{
	using System.Runtime.CompilerServices;

	public static class BoolExt
	{
		/*
		## Logical Implication

		Implication is a standard logical operation which is missing from practically
		all mainstream languages. So, we need to define it ourselves. The implication
		operator is defined mathematically as:

		$A \implies B \equiv \neg A \lor B$.

		Since this is a very simple expression replacement, we'll try to make the
		compiler inline it aggresively.
		*/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Implies(this bool antecedent, bool consequent) => 
			!antecedent || consequent;
	}
}

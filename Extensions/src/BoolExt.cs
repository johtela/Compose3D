namespace Extensions
{
	public static class BoolExt
	{
		public static bool Implies (this bool antecedent, bool consequent)
		{
			return !antecedent || consequent;
		}
	}
}

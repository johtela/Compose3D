namespace Extensions
{
	public static class NumExt
	{
		public static bool IsBetween (this int number, int floor, int ceil)
		{
			return number >= floor && number <= ceil;
		}
	}
}

namespace Extensions
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	public static class StringExt
	{
		public static string Times (this string what, int times)
		{
			var sb = new StringBuilder ();

			for (int i = 0; i < times; i++)
				sb.Append (what);

			return sb.ToString ();
		}
	}
}

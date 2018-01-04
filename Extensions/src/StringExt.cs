namespace Extensions
{
    using System.Collections.Generic;
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

        public static string CharsToString (this IEnumerable<char> chars)
        {
            var sb = new StringBuilder ();
            foreach (var ch in chars)
                sb.Append (ch);
            return sb.ToString ();
        }
	}
}

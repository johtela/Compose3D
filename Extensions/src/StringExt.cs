/*
# Extensions to Strings

The functionality of strings is fairly comprehensive in the .NET framework.
There are some additions we can still come up with.
*/
namespace Extensions
{
    using System.Collections.Generic;
	using System.Text;

	public static class StringExt
	{
		/*
		## Repeat a String _n_ Times

		The following method duplicates a string as many times as specified
		by the parameter.
		*/
		public static string Times (this string what, int times)
		{
			var sb = new StringBuilder ();

			for (int i = 0; i < times; i++)
				sb.Append (what);

			return sb.ToString ();
		}
		/*
		## IEnumerable<char> to String

		For some reason there is no ready, efficient way of converting
		an enumerable of chars into string. We provide that functionality
		with the `CharsToString` method.
		*/
        public static string CharsToString (this IEnumerable<char> chars)
        {
            var sb = new StringBuilder ();
            foreach (var ch in chars)
                sb.Append (ch);
            return sb.ToString ();
        }
	}
}

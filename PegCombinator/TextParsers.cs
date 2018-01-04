namespace PegCombinator
{
	using System;
	using System.Linq;
	using System.Text;
	using Extensions;
	using System.Collections.Generic;

	/// <summary>
	/// Parser combinator for characters and strings.
	/// </summary>
	public static class StringParser
	{
		/// <summary>
		/// Parse a given character.
		/// </summary>
		public static Parser<char, char> Char(char x) => 
			Parser.Satisfy<char>(y => x == y, x.ToString());

		/// <summary>
		/// Parse a number [0-9]
		/// </summary>
		public static Parser<char, char> Number() => 
			Parser.Satisfy<char>(char.IsNumber, "number");

		/// <summary>
		/// Parse a lower case character [a-z]
		/// </summary>
		public static Parser<char, char> Lower() => 
			Parser.Satisfy<char>(char.IsLower, "lowercase letter");

		/// <summary>
		/// Parse an upper case character [A-Z]
		/// </summary>
		public static Parser<char, char> Upper() => 
			Parser.Satisfy<char>(char.IsUpper, "uppercase letter");

		/// <summary>
		/// Parse any letter.
		/// </summary>
		public static Parser<char, char> Letter() => 
			Parser.Satisfy<char>(char.IsLetter, "letter");

		/// <summary>
		/// Parse on alphanumeric character.
		/// </summary>
		public static Parser<char, char> AlphaNumeric() => 
			Parser.Satisfy<char>(char.IsLetterOrDigit, "alphanumeric character");

		/// <summary>
		/// Parse a word (sequence of consequtive letters)
		/// </summary>
		/// <returns></returns>
		public static Parser<string, char> Word() => 
			from xs in Letter().OneOrMore()
			select xs.ToString("", "", "");

		/// <summary>
		/// Parse a character that is in the set of given characters.
		/// </summary>
		public static Parser<char, char> OneOf(params char[] chars) => 
			Parser.Satisfy<char>(c => chars.Contains(c),
				"any of the chars: " + chars.SeparateWith(", "));

		/// <summary>
		/// Parse a character that is NOT in the set of given characters.
		/// </summary>
		public static Parser<char, char> NoneOf(params char[] chars) => 
			Parser.Satisfy<char>(c => !chars.Contains(c),
				"any of character except: " + chars.SeparateWith(", "));

		/// <summary>
		/// Parse a given string.
		/// </summary>
		public static Parser<string, char> String(string str) =>
			str.Select(Char).Aggregate(Parser.Then).Then(str.Lift<string, char> ());

		/// <summary>
		/// Convenience method that combines character parser to a string parser.
		/// </summary>
		public static Parser<string, char> ManyChars(this Parser<char, char> parser) => 
			from seq in parser.OneOrMore()
			select seq.ToString("", "", "");

		/// <summary>
		/// Parse a positive integer without a leading '+' character.
		/// </summary>
		public static Parser<int, char> PositiveInteger(int defaultValue)
		{
			int res;
			return from x in Number().OneOrMore()
				   select int.TryParse(x.ToString(), out res) ? res : defaultValue;
		}

		/// <summary>
		/// Parse a possibly negative integer.
		/// </summary>
		public static Parser<int, char> Integer(int defaultValue) => 
			from sign in Char('-').OptionalVal()
			from number in PositiveInteger(defaultValue)
			select sign.HasValue ? -number : number;

		/// <summary>
		/// Creates a parser that skips whitespace, i.e. just consumes white space 
		/// from the sequence but does not return anything.
		/// </summary>
		public static Parser<string, char> Whitespace()
		{
			return from chars in Parser.Satisfy<char>(char.IsWhiteSpace).ZeroOrMore()
				   select chars.ToString ("", "", "");
		}

		public static Parser<T, char> SkipWhitespace<T>(this Parser<T, char> parser)
		{
			return from _ in Whitespace()
				   from v in parser
				   select v;
		}

		public static Parser<string, char> Identifier()
		{
			return from x in Letter()
				   from xs in AlphaNumeric().ZeroOrMore()
				   select (x | xs).ToString("", "", "");
		}
	}
}

namespace PegCombinator
{
    using System;
    using System.Linq;

    public class ParseError : Exception
    {
        public ParseError (string msg) : base (msg) { }

        public static ParseError FromParseResult<T> (ParseResult<T> result)
        {
            return new ParseError (string.Format (
                "Parse error at {0}\nUnexpected \"{1}\"\nExpected {2}",
                result.Position.ToString (), result.Found,
                result.Expected.Aggregate ("", (a, b) => a + " or " + b)));
        }
    }
}
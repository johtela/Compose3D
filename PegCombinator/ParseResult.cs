namespace PegCombinator
{
    using System;
    using Extensions;

    public abstract class ParseResult<T>
    {
        public abstract T Result { get; }
        public abstract bool ConsumedInput { get; }
        public abstract object Position { get; }
        public abstract string Found { get; }
        public abstract Seq<string> Expected { get; protected set; }

        private class Ok : ParseResult<T>
        {
            private T _result;
            private bool _consumedInput;

            public Ok (T result, bool consumedInput)
            {
                _result = result;
                _consumedInput = consumedInput;
            }

            public override T Result => _result;

            public override bool ConsumedInput => _consumedInput;

            public override object Position => 
                throw new InvalidOperationException ("Position not available");

            public override string Found => 
                throw new InvalidOperationException ("Terminal not available");

            public override Seq<string> Expected
            {
                get => null;
                protected set => throw new InvalidOperationException ("Expexted terminals not available");
            }
        }

        private class Fail : ParseResult<T>
        {
            private object _position;
            private string _found;
            private Seq<string> _expected;

            public Fail (object position, string found, Seq<string> expected)
            {
                _position = position;
                _found = found;
                _expected = expected;
            }

            public override T Result => 
                throw new InvalidOperationException ("Result not available");

            public override bool ConsumedInput => false;

            public override object Position => _position;

            public override string Found => _found;

            public override Seq<string> Expected
            {
                get => _expected;
                protected set => _expected = value;
            }
        }

        public Seq<string> MergeExpected (ParseResult<T> other)
        {
            var result = Expected;
            if (other is Fail)
                foreach (var exp in other.Expected)
                    result = exp | result;
            return result;
        }

        public static implicit operator bool (ParseResult<T> result)
        {
            return result is Ok;
        }

        public static ParseResult<T> Succeeded (T result, bool consumedInput)
        {
            return new Ok (result, false);
        }

        public static ParseResult<T> Failed (object position, string found)
        {
            return new Fail (position, found, null);
        }

        public static ParseResult<T> Failed (object position, string found, Seq<string> expected)
        {
            return new Fail (position, found, expected);
        }
    }
}
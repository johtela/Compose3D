namespace PegCombinator
{
    using System;

    public static class CollectionParsers
    {
        ///// <summary>
        ///// Creates a parser that will read a list of items separated by a separator.
        ///// The list needs to have at least one item.
        ///// </summary>
        //public static Parser<StrictList<T>, S> SeparatedBy1<T, U, S> (this Parser<T, S> parser,
        //    Parser<U, S> separator)
        //{
        //    return from x in parser
        //           from xs in
        //               (from y in separator.Seq (parser)
        //                select y).ZeroOrMore ()
        //           select x | xs;
        //}

        ///// <summary>
        ///// Creates a parser that will read a list of items separated by a separator.
        ///// The list can also be empty.
        ///// </summary>
        //public static Parser<StrictList<T>, S> SeparatedBy<T, U, S> (this Parser<T, S> parser,
        //    Parser<U, S> separator)
        //{
        //    return SeparatedBy1 (parser, separator).Plus (
        //        StrictList<T>.Empty.ToParser<StrictList<T>, S> ());
        //}

        ///// <summary>
        ///// Creates a parser the reads a bracketed input.
        ///// </summary>
        //public static Parser<T, S> Bracket<T, U, V, S> (this Parser<T, S> parser,
        //    Parser<U, S> open, Parser<V, S> close)
        //{
        //    return from o in open
        //           from x in parser
        //           from c in close
        //           select x;
        //}

        ///// <summary>
        ///// Creates a parser that reads an expression with multiple terms separated
        ///// by an operator. The operator is returned as a function and the terms are
        ///// evaluated left to right.
        ///// </summary>
        //public static Parser<T, S> ChainLeft1<T, S> (this Parser<T, S> parser,
        //    Parser<Func<T, T, T>, S> operation)
        //{
        //    return from x in parser
        //           from fys in
        //               (from f in operation
        //                from y in parser
        //                select new { f, y }).ZeroOrMore ()
        //           select fys.ReduceLeft (x, (z, fy) => fy.f (z, fy.y));
        //}

        ///// <summary>
        ///// Creates a parser that reads an expression with multiple terms separated
        ///// by an operator. The operator is returned as a function and the terms are
        ///// evaluated right to left.
        ///// </summary>
        //public static Parser<T, S> ChainRight1<T, S> (this Parser<T, S> parser,
        //    Parser<Func<T, T, T>, S> operation)
        //{
        //    return parser.Bind (x =>
        //           (from f in operation
        //            from y in ChainRight1 (parser, operation)
        //            select f (x, y))
        //            .Plus (x.ToParser<T, S> ())
        //    );
        //}

        ///// <summary>
        ///// Creates a parser that reads an expression with multiple terms separated
        ///// by an operator. The operator is returned as a function and the terms are
        ///// evaluated left to right. If the parsing of the expression fails, the value
        ///// given as an argument is returned as a parser.
        ///// </summary>
        //public static Parser<T, S> ChainLeft<T, S> (this Parser<T, S> parser,
        //    Parser<Func<T, T, T>, S> operation, T value)
        //{
        //    return parser.ChainLeft1 (operation).Plus (value.ToParser<T, S> ());
        //}

        ///// <summary>
        ///// Creates a parser that reads an expression with multiple terms separated
        ///// by an operator. The operator is returned as a function and the terms are
        ///// evaluated right to left. If the parsing of the expression fails, the value
        ///// given as an argument is returned as a parser.
        ///// </summary>
        //public static Parser<T, S> ChainRight<T, S> (this Parser<T, S> parser,
        //    Parser<Func<T, T, T>, S> operation, T value)
        //{
        //    return parser.ChainRight1 (operation).Plus (value.ToParser<T, S> ());
        //}

        ///// <summary>
        ///// Create a combined parser that will parse any of the given operators. 
        ///// The operators are specified in a seqeunce which contains (parser, result)
        ///// pairs. If the parser succeeds the result is returned, otherwise the next 
        ///// parser in the sequence is tried.
        ///// </summary>
        //public static Parser<U, S> Operators<T, U, S> (ISequence<Tuple<Parser<T, S>, U>> ops)
        //{
        //    return ops.Map (op => from _ in op.Item1
        //                          select op.Item2).ReduceLeft1 (Plus);
        //}
    }
}

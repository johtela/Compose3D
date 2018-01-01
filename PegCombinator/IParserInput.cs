namespace PegCombinator
{
    using System.Collections.Generic;

    public interface IParserInput<S> : IEnumerator<S>
    {
        object Position { get; set; }
    }
}

using System;
using System.Collections.Generic;
using Scoop.Parsers;

namespace Scoop.Tokenization
{
    public static class TokenParserMethods
    {
        public static IParser<TInput, TOutput> Match<TInput, TOutput>(IEnumerable<TInput> c, Func<TInput[], TOutput> produce)
        {
            return new MatchSequenceParser<TInput, TOutput>(c, produce);
        }
    }
}
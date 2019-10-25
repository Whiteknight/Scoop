using System;
using System.Collections.Generic;
using Scoop.Parsing.Parsers;

namespace Scoop.Parsing.Tokenization.Parsers
{
    public static class TokenParserMethods
    {
        public static IParser<TInput, TOutput> Match<TInput, TOutput>(IEnumerable<TInput> c, Func<TInput[], TOutput> produce)
        {
            return new MatchSequenceParser<TInput, TOutput>(c, produce);
        }
    }
}
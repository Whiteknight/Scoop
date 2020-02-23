using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects;
using ParserObjects.Parsers;

namespace Scoop.Parsing.Tokenization.Parsers
{
    public static class TokenParserMethods
    {
        public static IParser<TInput, TOutput> Match<TInput, TOutput>(IEnumerable<TInput> c, Func<TInput[], TOutput> produce)
        {
            return new MatchSequenceParser<TInput>(c).Transform(x => produce(x.ToArray()));
        }
    }
}
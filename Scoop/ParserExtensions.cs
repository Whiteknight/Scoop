using ParserObjects;
using ParserObjects.Sequences;
using Scoop.Parsing.Tokenization;

namespace Scoop
{
    public static class ParserExtensions
    {
        /// <summary>
        /// Convenience method to handle parsing a string into an AstNode
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="parser"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        public static TOutput Parse<TOutput>(this IParser<Token, TOutput> parser, string s)
        {
            var tokenizer = LexicalGrammar.GetParser()
                .ToSequence(new StringCharacterSequence(s))
                .Select(r => r.Value);
            return parser.Parse(tokenizer).Value;
        }
    }
}

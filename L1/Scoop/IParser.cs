using System;
using Scoop.Tokenization;

namespace Scoop
{
    public interface IParser<out TOutput>
    {
        TOutput TryParse(ITokenizer t);
        string Name { get; set; }
    }

    public static class ParserExtensions
    {
        public static ParseResult<TOutput> Parse<TOutput>(this IParser<TOutput> parser, ITokenizer t)
        {
            var window = t.Mark();
            try
            {
                var output = parser.TryParse(window);
                if (output != null)
                    return ParseResult<TOutput>.Success(output, parser.ToString());
                (window as WindowTokenizer)?.Rewind();
                return ParseResult<TOutput>.Fail(parser.ToString());
            }
            catch (Exception e)
            {
                (window as WindowTokenizer)?.Rewind();
                return ParseResult<TOutput>.Fail(parser.ToString(), e);
            }
        }

        public static TOutput Parse<TOutput>(this IParser<TOutput> parser, string s) => Parse(parser, new Tokenizer(s)).GetResult();

        public static IParser<TOutput> Named<TOutput>(this IParser<TOutput> parser, string name)
        {
            parser.Name = name;
            return parser;
        }
    }
}
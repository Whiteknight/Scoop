using System;
using System.Collections.Generic;
using Scoop.Parsers.Visiting;
using Scoop.Tokenization;

namespace Scoop.Parsers
{
    public interface IParser
    {
        string Name { get; set; }
        IParser Accept(IParserVisitorImplementation visitor);
        IEnumerable<IParser> GetChildren();
        IParser ReplaceChild(IParser find, IParser replace);
    }

    public interface IParser<out TOutput> : IParser
    {
        TOutput TryParse(ITokenizer t);
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
            catch (TokenizingException)
            {
                // Tokenizer exceptions indicate that there isn't a valid token to get. The parse cannot
                // continue no matter what. We propagate those all the way up to the top
                throw;
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

        public static IParser<TOutput> Replace<TOutput>(this IParser<TOutput> root, Func<IParser, bool> predicate, IParser replacement) 
            => new ReplaceParserVisitor(predicate, replacement).Visit(root) as IParser<TOutput>;

        public static IParser FindNamed(this IParser root, string name) => FindParserVisitor.Named(name, root);

        public static IParser<TOutput> Replace<TOutput>(this IParser<TOutput> root, IParser find, IParser replace) 
            => new ReplaceParserVisitor(p => p == find, replace).Visit(root) as IParser<TOutput>;
    }
}
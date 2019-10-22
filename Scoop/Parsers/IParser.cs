using System;
using System.Collections.Generic;
using Scoop.Parsers.Visiting;
using Scoop.SyntaxTree;
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

    public interface IParser<TInput> : IParser
    {
        IParseResult<object> ParseUntyped(ISequence<TInput> t);
    }

    public interface IParser<TInput, out TOutput> : IParser<TInput>
    {
        IParseResult<TOutput> Parse(ISequence<TInput> t);
    }

    public static class ParserExtensions
    {
        //public static ParseResult<TOutput> Parse<TOutput>(this IParser<TInput, TOutput> parser, ISequence<TInput> t)
        //{
        //    var window = t.Mark();
        //    try
        //    {
        //        var output = parser.TryParse(window);
        //        if (output != null)
        //            return ParseResult<TOutput>.Success(output, parser.ToString());
        //        (window as WindowTokenizer)?.Rewind();
        //        return ParseResult<TOutput>.Fail(parser.ToString());
        //    }
        //    catch (TokenizingException)
        //    {
        //        // Tokenizer exceptions indicate that there isn't a valid token to get. The parse cannot
        //        // continue no matter what. We propagate those all the way up to the top
        //        throw;
        //    }
        //    catch (Exception e)
        //    {
        //        (window as WindowTokenizer)?.Rewind();
        //        return ParseResult<TOutput>.Fail(parser.ToString(), e);
        //    }
        //}

        public static TOutput Parse<TOutput>(this IParser<Token, TOutput> parser, string s)
            where TOutput : AstNode 
            => parser.Parse(new Tokenizer(s)).Value;

        public static IParser<TInput, TOutput> Named<TInput, TOutput>(this IParser<TInput, TOutput> parser, string name)
            where TOutput : AstNode
        {
            parser.Name = name;
            return parser;
        }

        public static IParser<TInput, TOutput> Replace<TInput, TOutput>(this IParser<TInput, TOutput> root, Func<IParser, bool> predicate, IParser replacement)
            where TOutput : AstNode
            => new ReplaceParserVisitor(predicate, replacement).Visit(root) as IParser<TInput, TOutput>;

        public static IParser FindNamed(this IParser root, string name) => FindParserVisitor.Named(name, root);

        public static IParser<TInput, TOutput> Replace<TInput, TOutput>(this IParser<TInput, TOutput> root, IParser find, IParser replace)
            where TOutput : AstNode
            => new ReplaceParserVisitor(p => p == find, replace).Visit(root) as IParser<TInput, TOutput>;
    }
}
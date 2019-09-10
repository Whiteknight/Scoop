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

    public struct ParseResult<TOutput>
    {
        public bool IsSuccess { get; }
        public TOutput Value { get; }
        public Exception Exception { get; }
        public string Name { get; }

        public ParseResult(bool success, TOutput value, string name, Exception e)
        {
            IsSuccess = success;
            Value = value;
            Name = name;
            Exception = e;
        }

        public static ParseResult<TOutput> Success(TOutput value, string name)
        {
            return new ParseResult<TOutput>(true, value, name, null);
        }

        public static ParseResult<TOutput> Fail(string name)
        {
            return new ParseResult<TOutput>(false, default(TOutput), name, null);
        }

        public static ParseResult<TOutput> Fail(string name, Exception e)
        {
            return new ParseResult<TOutput>(false, default(TOutput), name, e);
        }

        public TOutput GetResult()
        {
            if (!IsSuccess)
            {
                // TODO: Get a better exception type
                if (Exception != null)
                    throw new Exception($"Parse fail rule={Name ?? "unnamed"} output type={typeof(TOutput).Name}", Exception);
                throw new Exception($"Parse fail rule={Name ?? "unnamed"} output type={typeof(TOutput).Name}");
            }
            return Value;
        }
    }
}
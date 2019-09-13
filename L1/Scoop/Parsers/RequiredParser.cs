using System;
using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop.Parsers
{
    public class RequiredParser<TOutput> : IParser<TOutput>
        where TOutput : AstNode
    {
        private readonly IParser<TOutput> _inner;
        private readonly Func<Location, TOutput> _otherwise;

        public RequiredParser(IParser<TOutput> inner, Func<Location, TOutput> otherwise)
        {
            _inner = inner;
            _otherwise = otherwise;
        }

        public TOutput TryParse(ITokenizer t)
        {
            // .Parse() here so _inner rewinds on failure
            var result = _inner.Parse(t);
            if (result.IsSuccess)
                return result.Value;

            // Otherwise, create the fallback production at the location where _inner would have started
            return _otherwise(t.Peek().Location);
        }

        public string Name { get; set; }

        public override string ToString()
        {
            return $"Required.{_inner}";
        }
    }
}

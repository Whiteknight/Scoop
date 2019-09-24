using System;
using System.Collections.Generic;
using Scoop.Parsers.Visiting;
using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop.Parsers
{
    /// <summary>
    /// Parser to require a valid output. If the inner parser fails, a default fallback value
    /// is constructed and returned.
    /// The default fallback value might be an error/diagnostic object to help with error reporting
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
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

        public IParser Accept(IParserVisitorImplementation visitor) => visitor.VisitRequired(this);

        public IEnumerable<IParser> GetChildren() => new[] { _inner };

        public IParser ReplaceChild(IParser find, IParser replace)
        {
            if (_inner == find && replace is IParser<TOutput> realReplace)
                return new RequiredParser<TOutput>(realReplace, _otherwise);
            return this;
        }

        public override string ToString()
        {
            return $"Required.{_inner}";
        }
    }
}

using System.Collections.Generic;
using Scoop.Parsers.Visiting;
using Scoop.Tokenization;

namespace Scoop.Parsers
{
    // Delegates to an internal parser, and also allows the internal parser to be
    // replaced without causing the entire parser tree to be rewritten.
    // Also if a child has been rewritten and the rewrite is bubbling up the tree, it will
    // stop here.
    public class ReplaceableParser<TOutput> : IParser<TOutput>
    {
        private IParser<TOutput> _value;

        public ReplaceableParser(IParser<TOutput> defaultValue)
        {
            _value = defaultValue;
        }

        public TOutput Parse(ITokenizer t) => _value.Parse(t);

        public string Name { get; set; }

        public IParser Accept(IParserVisitorImplementation visitor) => visitor.VisitReplaceable(this);

        public IEnumerable<IParser> GetChildren() => new[] { _value };

        public IParser ReplaceChild(IParser find, IParser replace)
        {
            if (_value == find && replace is IParser<TOutput> realReplace)
                _value = realReplace;
            return this;
        }

        public void SetParser(IParser<TOutput> parser)
        {
            _value = parser;
        }
    }
}
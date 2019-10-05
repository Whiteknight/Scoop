using System.Collections.Generic;
using System.Linq;
using Scoop.Parsers.Visiting;
using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop.Parsers
{
    /// <summary>
    /// Parses one of a list of allowable keywords
    /// </summary>
    public class KeywordParser : IParser<KeywordNode>
    {
        private readonly string[] _expected;

        public KeywordParser(params string[] expected)
        {
            _expected = expected;
        }

        public KeywordNode Parse(ITokenizer t)
        {
            var word = t.Peek();
            if (!word.IsType(TokenType.Word))
                return null;

            if (_expected.Any() && !_expected.Contains(word.Value))
                return null;

            return new KeywordNode(t.GetNext());
        }

        public string Name { get; set; }

        public IParser Accept(IParserVisitorImplementation visitor) => visitor.VisitKeyword(this);

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public IParser ReplaceChild(IParser find, IParser replace) => this;

        public override string ToString()
        {
            var typeName = this.GetType().Name;
            return Name == null ? base.ToString() : $"{typeName} {Name}";
        }
    }
}

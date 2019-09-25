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

        public KeywordNode TryParse(ITokenizer t)
        {
            var word = t.GetNext();
            // It must be a keyword and it must be one of the keywords we're looking for
            if (!word.IsType(TokenType.Word))
            {
                t.PutBack(word);
                return null;
            }

            if (_expected.Any() && !_expected.Contains(word.Value))
            {
                t.PutBack(word);
                return null;
            }

            return new KeywordNode(word);
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

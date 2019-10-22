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
    public class KeywordParser : IParser<Token, KeywordNode>
    {
        private readonly string[] _expected;

        public KeywordParser(params string[] expected)
        {
            _expected = expected;
        }

        public IParseResult<KeywordNode> Parse(ISequence<Token> t)
        {
            var word = t.Peek();
            if (!word.IsType(TokenType.Word))
                return Result<KeywordNode>.Fail();

            if (_expected.Any() && !_expected.Contains(word.Value))
                return Result<KeywordNode>.Fail();

            return new Result<KeywordNode>(true, new KeywordNode(t.GetNext()));
        }

        IParseResult<object> IParser<Token>.ParseUntyped(ISequence<Token> t) => (IParseResult<object>)Parse(t);

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

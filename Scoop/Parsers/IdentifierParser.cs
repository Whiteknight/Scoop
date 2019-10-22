using System.Collections.Generic;
using System.Linq;
using Scoop.Parsers.Visiting;
using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop.Parsers
{
    /// <summary>
    /// Attempts to parse an identifier
    /// </summary>
    public class IdentifierParser : IParser<Token, IdentifierNode>
    {
        private readonly HashSet<string> _keywords;
        private readonly string[] _identifiers;

        public IdentifierParser(HashSet<string> keywords, params string[] identifiers)
        {
            _keywords = keywords;
            _identifiers = identifiers;
        }

        public IParseResult<IdentifierNode> Parse(ISequence<Token> t)
        {
            var id = t.Peek();
            // If it's not a Word or if the value isn't a keyword, fail
            if (!id.IsType(TokenType.Word) || _keywords.Contains(id.Value))
                return Result<IdentifierNode>.Fail();

            if (_identifiers == null || _identifiers.Length == 0)
                return new Result<IdentifierNode>(true, new IdentifierNode(t.GetNext()));

            if (_identifiers.Any(identifier => identifier == id.Value))
                return new Result<IdentifierNode>(true, new IdentifierNode(t.GetNext()));

            return Result<IdentifierNode>.Fail();
        }

        IParseResult<object> IParser<Token>.ParseUntyped(ISequence<Token> t) => (IParseResult<object>)Parse(t);

        public string Name { get; set; }

        public IParser Accept(IParserVisitorImplementation visitor) => visitor.VisitIdentifier(this);

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public IParser ReplaceChild(IParser find, IParser replace) => this;

        public override string ToString()
        {
            var typeName = this.GetType().Name;
            return Name == null ? base.ToString() : $"{typeName} {Name}";
        }
    }
}
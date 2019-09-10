using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop.Parsers
{
    public class IdentifierParser : IParser<IdentifierNode>
    {
        private readonly string[] _identifiers;

        public IdentifierParser(params string[] identifiers)
        {
            _identifiers = identifiers;
        }

        public IdentifierNode TryParse(ITokenizer t)
        {
            var id = t.Peek();
            if (!id.IsType(TokenType.Identifier))
                return null;
            if (_identifiers != null && _identifiers.Length > 0)
            {
                foreach (var identifier in _identifiers)
                {
                    if (identifier == id.Value)
                    {
                        t.Advance();
                        return new IdentifierNode(id);
                    }
                }

                return null;
            }

            t.Advance();
            return new IdentifierNode(id);
        }

        public string Name { get; set; }
        public override string ToString()
        {
            var typeName = this.GetType().Name;
            return Name == null ? base.ToString() : $"{typeName} {Name}";
        }
    }
}
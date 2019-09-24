﻿using System.Collections.Generic;
using System.Linq;
using Scoop.Parsers.Visiting;
using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop.Parsers
{
    /// <summary>
    /// Attempts to parse an identifier
    /// </summary>
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

            if (_identifiers == null || _identifiers.Length <= 0)
                return new IdentifierNode(t.GetNext());

            if (_identifiers.Any(identifier => identifier == id.Value))
                return new IdentifierNode(t.GetNext());

            return null;
        }

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
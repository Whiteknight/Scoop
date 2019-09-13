using System.Collections.Generic;
using Scoop.Tokenization;

namespace Scoop.SyntaxTree
{
    // Simple identifier which cannot be qualified with '.'
    public class IdentifierNode : AstNode
    {
        public string Id { get; }

        public IdentifierNode(Token t, IReadOnlyList<Diagnostic> diagnostics = null) : base(diagnostics)
        {
            if (t.Type != TokenType.Identifier)
                throw ParsingException.UnexpectedToken(TokenType.Identifier, t);
            Location = t.Location;
            Id = t.Value;
        }

        public IdentifierNode(string id, Location location = null, IReadOnlyList<Diagnostic> diagnostics = null) : base(diagnostics)
        {
            Id = id;
            Location = location;
        }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitIdentifier(this);
    }
}
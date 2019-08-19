using Scoop.Tokenization;

namespace Scoop.SyntaxTree
{
    // Simple identifier which cannot be qualified with '.'
    public class IdentifierNode : AstNode
    {
        public string Id { get; }

        public IdentifierNode(Token t)
        {
            if (t.Type != TokenType.Identifier)
                throw ParsingException.UnexpectedToken(TokenType.Identifier, t);
            Location = t.Location;
            Id = t.Value;
        }

        public IdentifierNode(string id, Location location = null)
        {
            Id = id;
            Location = location;
        }

        public override AstNode Accept(AstNodeVisitor visitor) => visitor.VisitIdentifier(this);
    }
}
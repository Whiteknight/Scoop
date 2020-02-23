using ParserObjects;
using Scoop.Parsing.Tokenization;
using Scoop.SyntaxTree.Visiting;

namespace Scoop.SyntaxTree
{
    // Simple identifier which cannot be qualified with '.'
    public class IdentifierNode : AstNode
    {
        public string Id { get; }

        public IdentifierNode()
        {
        }

        public IdentifierNode(Token t)
        {
            Location = t.Location;
            Id = t.Value;
        }

        public IdentifierNode(string id, Location location = null)
        {
            Id = id;
            Location = location;
        }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitIdentifier(this);
    }
}
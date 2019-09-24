using System.Collections.Generic;
using System.Linq;
using Scoop.SyntaxTree.Visiting;

namespace Scoop.SyntaxTree
{
    public class DottedIdentifierNode : AstNode
    {
        public string Id { get; }
        public IReadOnlyList<string> Parts { get; }

        public DottedIdentifierNode(IEnumerable<string> parts, Location location = null)
        {
            Parts = parts.ToList();
            Id = string.Join(".", Parts);
            Location = location;
        }

        public DottedIdentifierNode(string id, Location location = null)
        {
            // TODO: global:: ?
            Id = id;
            Parts = Id.Split('.');
            Location = location;
        }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitDottedIdentifier(this);
    }
}
using System.Collections.Generic;
using System.Linq;
using Scoop.Tokenization;

namespace Scoop.SyntaxTree
{
    public class DottedIdentifierNode : AstNode
    {
        public string Id { get; }
        public IReadOnlyList<string> Parts { get; }

        public DottedIdentifierNode(Token t, IReadOnlyList<Diagnostic> diagnostics = null) : base(diagnostics)
        {
            Location = t.Location;
            Id = t.Value;
            Parts = Id.Split('.');
        }

        public DottedIdentifierNode(IEnumerable<string> parts, Location location = null, IReadOnlyList<Diagnostic> diagnostics = null) : base(diagnostics)
        {
            Parts = parts.ToList();
            Id = string.Join(".", Parts);
            Location = location;
        }

        public DottedIdentifierNode(string id, Location location = null, IReadOnlyList<Diagnostic> diagnostics = null) : base(diagnostics)
        {
            // TODO: global:: ?
            Id = id;
            Parts = Id.Split('.');
            Location = location;
        }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitDottedIdentifier(this);
    }
}
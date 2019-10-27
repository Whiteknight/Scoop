using System.Linq;
using Scoop.Parsing.Tokenization;
using Scoop.SyntaxTree.Visiting;

namespace Scoop.SyntaxTree
{
    public class StringNode : AstNode
    {
        public StringNode(string value, Location location = null)
        {
            Value = value;
            Location = location;
        }

        public StringNode(Token t)
        {
            Value = t.Value;
            Location = t.Location;
            if (!t.Diagnostics.IsNullOrEmpty())
                AddDiagnostics(t.Diagnostics.ToArray());
        }

        public string Value { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitString(this);
    }
}
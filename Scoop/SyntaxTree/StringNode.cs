using Scoop.SyntaxTree.Visiting;
using Scoop.Tokenization;

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
        }

        public string Value { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitString(this);
    }
}
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

        public bool Literal { get; set; }
        public bool Interpolated { get; set; }

        public string Value { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitString(this);
    }
}
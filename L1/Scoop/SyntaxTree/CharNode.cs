using Scoop.Tokenization;

namespace Scoop.SyntaxTree
{
    public class CharNode : AstNode
    {
        public CharNode(char value, Location location = null)
        {
            Value = value;
            Location = location;
        }

        public CharNode(Token t)
        {
            Value = t.Value[0];
            Location = t.Location;
        }

        public char Value { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitChar(this);
    }
}
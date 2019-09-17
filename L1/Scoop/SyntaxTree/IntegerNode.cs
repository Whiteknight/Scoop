using Scoop.Tokenization;

namespace Scoop.SyntaxTree
{
    public class IntegerNode : AstNode
    {
        public IntegerNode(int value)
        {
            Value = value;
        }

        public IntegerNode(Token t)
        {
            Value = int.Parse(t.Value);
            Location = t.Location;
        }

        public int Value { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitInteger(this);
    }
}
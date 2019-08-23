using Scoop.Tokenization;

namespace Scoop.SyntaxTree
{
    public class OperatorNode : AstNode
    {
        public OperatorNode(Token t)
        {
            Operator = t.Value;
            Location = t.Location;
        }

        public OperatorNode(string op, Location location = null)
        {
            Operator = op;
            Location = location;
        }

        public string Operator { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitOperator(this);
    }
}

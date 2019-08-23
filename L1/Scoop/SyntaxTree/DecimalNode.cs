using Scoop.Tokenization;

namespace Scoop.SyntaxTree
{
    public class DecimalNode : AstNode
    {
        public DecimalNode(decimal value)
        {
            Value = value;
        }

        public DecimalNode(Token t)
        {
            Value = decimal.Parse(t.Value);
            Location = t.Location;
        }

        public decimal Value { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitDecimal(this);
    }
}
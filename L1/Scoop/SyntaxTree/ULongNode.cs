using Scoop.Tokenization;

namespace Scoop.SyntaxTree
{
    public class ULongNode : AstNode
    {
        public ULongNode(ulong value)
        {
            Value = value;
        }

        public ULongNode(Token t)
        {
            Value = ulong.Parse(t.Value);
            Location = t.Location;
        }

        public ulong Value { get; set; }

        public override AstNode Accept(AstNodeVisitor visitor) => visitor.VisitULong(this);
    }
}
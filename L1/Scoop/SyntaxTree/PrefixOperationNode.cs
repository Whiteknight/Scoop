namespace Scoop.SyntaxTree
{
    public class PrefixOperationNode : AstNode
    {
        public OperatorNode Operator { get; set; }
        public AstNode Right { get; set; }

        public override AstNode Accept(AstNodeVisitor visitor) => visitor.VisitPrefixOperation(this);
    }

    public class CastNode : AstNode
    {
        public TypeNode Type { get; set; }
        public AstNode Right { get; set; }

        public override AstNode Accept(AstNodeVisitor visitor) => visitor.VisitCast(this);
    }
}
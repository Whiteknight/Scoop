namespace Scoop.SyntaxTree
{
    public class PrefixOperationNode : AstNode
    {
        public OperatorNode Operator { get; set; }
        public AstNode Right { get; set; }

        public override AstNode Accept(AstNodeVisitor visitor) => visitor.VisitPrefixOperation(this);
    }
}
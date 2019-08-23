namespace Scoop.SyntaxTree
{
    public class PostfixOperationNode : AstNode
    {
        public AstNode Left { get; set; }
        public OperatorNode Operator { get; set; }
        public override AstNode Accept(AstNodeVisitor visitor) => visitor.VisitPostfixOperation(this);
    }
}
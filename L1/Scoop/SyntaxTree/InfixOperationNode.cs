namespace Scoop.SyntaxTree
{
    public class InfixOperationNode : AstNode
    {
        public AstNode Left { get; set; }
        public OperatorNode Operator { get; set; }
        public AstNode Right { get; set; }

        public override AstNode Accept(AstNodeVisitor visitor) => visitor.VisitInfixOperation(this);
    }
}
namespace Scoop.SyntaxTree
{
    public class PrefixOperationNode : AstNode
    {
        public OperatorNode Operator { get; set; }
        public AstNode Right { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitPrefixOperation(this);
    }
}
namespace Scoop.SyntaxTree
{
    public class PrefixOperationNode : AstNode
    {
        public OperatorNode Operator { get; set; }
        public AstNode Right { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitPrefixOperation(this);
    }

    public class CastNode : AstNode
    {
        public TypeNode Type { get; set; }
        public AstNode Right { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitCast(this);
    }
}
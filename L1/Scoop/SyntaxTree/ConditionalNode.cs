namespace Scoop.SyntaxTree
{
    public class ConditionalNode : AstNode
    {
        public AstNode Condition { get; set; }
        public AstNode IfTrue { get; set; }
        public AstNode IfFalse { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitConditional(this);
    }
}

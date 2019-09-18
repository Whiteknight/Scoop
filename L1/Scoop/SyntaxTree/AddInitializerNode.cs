namespace Scoop.SyntaxTree
{
    public class AddInitializerNode : AstNode
    {
        public ListNode<AstNode> Arguments { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitAddInitializer(this);
    }
}

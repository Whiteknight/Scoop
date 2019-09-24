using Scoop.SyntaxTree.Visiting;

namespace Scoop.SyntaxTree
{
    public class InvokeNode : AstNode
    {
        public AstNode Instance { get; set; }
        public ListNode<AstNode> Arguments { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitInvoke(this);
    }
}

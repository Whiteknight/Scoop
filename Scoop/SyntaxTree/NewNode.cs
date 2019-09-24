using Scoop.SyntaxTree.Visiting;

namespace Scoop.SyntaxTree
{
    public class NewNode : AstNode
    {
        public TypeNode Type { get; set; }
        public ListNode<AstNode> Arguments { get; set; }
        public ListNode<AstNode> Initializers { get; set; }
        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitNew(this);
    }
}
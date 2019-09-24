using Scoop.SyntaxTree.Visiting;

namespace Scoop.SyntaxTree
{
    public class IndexerInitializerNode : AstNode
    {
        public ListNode<AstNode> Arguments { get; set; }
        public AstNode Value { get; set; }
        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitIndexerInitializer(this);
    }
}
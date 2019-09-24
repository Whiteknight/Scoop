using Scoop.SyntaxTree.Visiting;

namespace Scoop.SyntaxTree
{
    public class NamespaceNode : AstNode
    {
        public ListNode<AttributeNode> Attributes { get; set; }

        public DottedIdentifierNode Name { get; set; }
        // classes, interfaces, etc
        public ListNode<AstNode> Declarations { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitNamespace(this);
    }
}
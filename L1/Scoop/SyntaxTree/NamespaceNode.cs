using System.Collections.Generic;

namespace Scoop.SyntaxTree
{
    public class NamespaceNode : AstNode, IHasAttributes
    {
        public NamespaceNode(IReadOnlyList<Diagnostic> diagnostics = null) : base(diagnostics)
        {
        }

        public ListNode<AttributeNode> Attributes { get; set; }

        public DottedIdentifierNode Name { get; set; }
        // classes, interfaces, etc
        public ListNode<AstNode> Declarations { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitNamespace(this);
    }
}
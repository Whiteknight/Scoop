using System.Collections.Generic;

namespace Scoop.SyntaxTree
{
    public class AttributeNode : AstNode
    {
        public AttributeNode(IReadOnlyList<Diagnostic> diagnostics = null) : base(diagnostics)
        {
        }

        public KeywordNode Target { get; set; }
        public TypeNode Type { get; set; }
        public ListNode<AstNode> Arguments { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitAttribute(this);
    }
}

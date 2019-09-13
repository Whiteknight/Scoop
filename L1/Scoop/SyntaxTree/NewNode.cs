using System.Collections.Generic;

namespace Scoop.SyntaxTree
{
    public class NewNode : AstNode
    {
        public NewNode(IReadOnlyList<Diagnostic> diagnostics = null) : base(diagnostics)
        {
        }

        public TypeNode Type { get; set; }
        public ListNode<AstNode> Arguments { get; set; }
        public ListNode<AstNode> Initializers { get; set; }
        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitNew(this);
    }
}
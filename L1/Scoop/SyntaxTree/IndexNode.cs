using System.Collections.Generic;

namespace Scoop.SyntaxTree
{
    public class IndexNode : AstNode
    {
        public IndexNode(IReadOnlyList<Diagnostic> diagnostics = null) : base(diagnostics)
        {
        }


        public AstNode Instance { get; set; }
        public ListNode<AstNode> Arguments { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitIndex(this);
    }
}

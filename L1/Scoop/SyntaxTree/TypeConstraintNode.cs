using System.Collections.Generic;

namespace Scoop.SyntaxTree
{
    public class TypeConstraintNode : AstNode
    {
        public TypeConstraintNode(IReadOnlyList<Diagnostic> diagnostics = null) : base(diagnostics)
        {
        }

        public IdentifierNode Type { get; set; }
        public ListNode<AstNode> Constraints { get; set; }
        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitTypeConstraint(this);
    }
}

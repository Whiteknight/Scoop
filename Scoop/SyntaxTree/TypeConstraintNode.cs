using Scoop.SyntaxTree.Visiting;

namespace Scoop.SyntaxTree
{
    public class TypeConstraintNode : AstNode
    {
        public IdentifierNode Type { get; set; }
        public ListNode<AstNode> Constraints { get; set; }
        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitTypeConstraint(this);
    }
}

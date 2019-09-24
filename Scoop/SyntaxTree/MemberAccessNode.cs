using Scoop.SyntaxTree.Visiting;

namespace Scoop.SyntaxTree
{
    public class MemberAccessNode : AstNode
    {
        public AstNode Instance { get; set; }
        public IdentifierNode MemberName { get; set; }
        public OperatorNode Operator { get; set; }
        public ListNode<TypeNode> GenericArguments { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitMemberAccess(this);
    }
}
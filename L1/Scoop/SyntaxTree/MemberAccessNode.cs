using System.Collections.Generic;

namespace Scoop.SyntaxTree
{
    public class MemberAccessNode : AstNode
    {
        public AstNode Instance { get; set; }
        public IdentifierNode MemberName { get; set; }
        public bool IgnoreNulls { get; set; }
        public ListNode<TypeNode> GenericArguments { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitMemberAccess(this);

    }
}
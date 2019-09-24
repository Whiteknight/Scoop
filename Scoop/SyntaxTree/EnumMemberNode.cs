using Scoop.SyntaxTree.Visiting;

namespace Scoop.SyntaxTree
{
    public class EnumMemberNode : AstNode
    {
        public ListNode<AttributeNode> Attributes { get; set; }
        public IdentifierNode Name { get; set; }
        public AstNode Value { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitEnumMember(this);
    }
}
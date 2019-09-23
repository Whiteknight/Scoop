namespace Scoop.SyntaxTree
{
    public class EnumNode : AstNode
    {
        public ListNode<AttributeNode> Attributes { get; set; }
        public KeywordNode AccessModifier { get; set; }
        public IdentifierNode Name { get; set; }
        public ListNode<EnumMemberNode> Members { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitEnum(this);
    }
}

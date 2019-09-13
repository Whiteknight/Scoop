using System.Collections.Generic;

namespace Scoop.SyntaxTree
{
    public class EnumNode : AstNode, IHasAttributes
    {
        public EnumNode(IReadOnlyList<Diagnostic> diagnostics = null) : base(diagnostics)
        {
        }

        public ListNode<AttributeNode> Attributes { get; set; }
        public KeywordNode AccessModifier { get; set; }
        public IdentifierNode Name { get; set; }
        public ListNode<EnumMemberNode> Members { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitEnum(this);
    }

    public class EnumMemberNode : AstNode, IHasAttributes
    {
        public EnumMemberNode(IReadOnlyList<Diagnostic> diagnostics = null) : base(diagnostics)
        {
        }

        public ListNode<AttributeNode> Attributes { get; set; }
        public IdentifierNode Name { get; set; }
        public AstNode Value { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitEnumMember(this);
    }
}

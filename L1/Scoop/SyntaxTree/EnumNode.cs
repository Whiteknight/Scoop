using System.Collections.Generic;

namespace Scoop.SyntaxTree
{
    public class EnumNode : AstNode, IHasAttributes
    {
        public List<AttributeNode> Attributes { get; set; }
        public KeywordNode AccessModifier { get; set; }
        public IdentifierNode Name { get; set; }
        public List<EnumMemberNode> Members { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitEnum(this);
    }

    public class EnumMemberNode : AstNode
    {
        public List<AttributeNode> Attributes { get; set; }
        public IdentifierNode Name { get; set; }
        public IntegerNode Value { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitEnumMember(this);
    }
}

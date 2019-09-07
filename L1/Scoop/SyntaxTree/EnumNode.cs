using System;
using System.Collections.Generic;
using System.Text;

namespace Scoop.SyntaxTree
{
    public class EnumNode : AstNode
    {
        public KeywordNode AccessModifier { get; set; }
        public IdentifierNode Name { get; set; }
        public List<EnumMemberNode> Members { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitEnum(this);
    }

    public class EnumMemberNode : AstNode
    {
        public IdentifierNode Name { get; set; }
        public IntegerNode Value { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitEnumMember(this);
    }
}

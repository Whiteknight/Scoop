using System.Collections.Generic;

namespace Scoop.SyntaxTree
{
    public class ParameterNode : AstNode, IHasAttributes
    {
        public ListNode<AttributeNode> Attributes { get; set; }
        public bool IsParams { get; set; }
        public AstNode Type { get; set; }
        public IdentifierNode Name { get; set; }
        public AstNode DefaultValue { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitParameter(this);
    }
}

using System.Collections.Generic;

namespace Scoop.SyntaxTree
{
    public class NewNode : AstNode
    {
        public AstNode Type { get; set; }
        public ListNode<AstNode> Arguments { get; set; }
        public ListNode<AstNode> Initializers { get; set; }
        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitNew(this);
    }
}
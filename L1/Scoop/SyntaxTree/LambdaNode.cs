using System.Collections.Generic;

namespace Scoop.SyntaxTree
{
    public class LambdaNode : AstNode
    {
        public ListNode<IdentifierNode> Parameters { get; set; }
        public ListNode<AstNode> Statements { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitLambda(this);
    }
}

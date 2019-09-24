using Scoop.SyntaxTree.Visiting;

namespace Scoop.SyntaxTree
{
    public class PostfixOperationNode : AstNode
    {
        public AstNode Left { get; set; }
        public OperatorNode Operator { get; set; }
        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitPostfixOperation(this);
    }
}
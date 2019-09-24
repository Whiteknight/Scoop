using Scoop.SyntaxTree.Visiting;

namespace Scoop.SyntaxTree
{
    public class EmptyNode : AstNode
    {
        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitEmpty(this);
    }
}

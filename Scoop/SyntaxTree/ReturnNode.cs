using Scoop.SyntaxTree.Visiting;

namespace Scoop.SyntaxTree
{
    public class ReturnNode : AstNode
    {
        public AstNode Expression { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitReturn(this);
    }
}
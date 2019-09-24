using Scoop.SyntaxTree.Visiting;

namespace Scoop.SyntaxTree
{
    public class NamedArgumentNode : AstNode
    {
        public IdentifierNode Name { get; set; }
        public OperatorNode Separator { get; set; }
        public AstNode Value { get; set; }
        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitNamedArgument(this);
    }
}

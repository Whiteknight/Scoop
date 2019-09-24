using Scoop.SyntaxTree.Visiting;

namespace Scoop.SyntaxTree
{
    public class TypeCoerceNode : AstNode
    {
        public AstNode Left { get; set; }
        public OperatorNode Operator { get; set; }
        public TypeNode Type { get; set; }
        public IdentifierNode Alias { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitTypeCoerce(this);
    }
}
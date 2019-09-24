using Scoop.SyntaxTree.Visiting;

namespace Scoop.SyntaxTree
{
    public class ConstNode : AstNode
    {
        public KeywordNode AccessModifier { get; set; }
        public TypeNode Type { get; set; }
        public IdentifierNode Name { get; set; }
        public AstNode Value { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitConst(this);
    }
}

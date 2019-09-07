namespace Scoop.SyntaxTree
{
    public class FieldNode : AstNode
    {
        public AstNode Type { get; set; }
        public IdentifierNode Name { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitField(this);
    }
}

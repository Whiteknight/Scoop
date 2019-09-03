namespace Scoop.SyntaxTree
{
    public class ParameterNode : AstNode
    {
        public AstNode Type { get; set; }
        public IdentifierNode Name { get; set; }
        public AstNode DefaultValue { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitParameter(this);
    }
}

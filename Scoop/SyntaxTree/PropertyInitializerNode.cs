namespace Scoop.SyntaxTree
{
    public class PropertyInitializerNode : AstNode
    {
        public IdentifierNode Property { get; set; }
        public AstNode Value { get; set; }
        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitPropertyInitializer(this);
    }
}
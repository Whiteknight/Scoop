namespace Scoop.SyntaxTree
{
    public class KeyValueInitializerNode : AstNode
    {
        public AstNode Key { get; set; }
        public AstNode Value { get; set; }
        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitKeyValueInitializer(this);
    }

    public class ArrayInitializerNode : AstNode
    {
        public IntegerNode Key { get; set; }
        public AstNode Value { get; set; }
        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitArrayInitializer(this);
    }

    public class PropertyInitializerNode : AstNode
    {
        public IdentifierNode Property { get; set; }
        public AstNode Value { get; set; }
        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitPropertyInitializer(this);
    }
}

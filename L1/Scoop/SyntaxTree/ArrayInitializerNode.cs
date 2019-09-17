namespace Scoop.SyntaxTree
{
    public class ArrayInitializerNode : AstNode
    {
        public IntegerNode Key { get; set; }
        public AstNode Value { get; set; }
        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitArrayInitializer(this);
    }
}
namespace Scoop.SyntaxTree
{
    public class KeyValueInitializerNode : AstNode
    {
        public AstNode Key { get; set; }
        public AstNode Value { get; set; }
        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitKeyValueInitializer(this);
    }
}

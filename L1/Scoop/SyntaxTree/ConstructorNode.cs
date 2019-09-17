namespace Scoop.SyntaxTree
{
    public class ConstructorNode : AstNode
    {
        // Represents a constructor, a special case of method
        public ListNode<AttributeNode> Attributes { get; set; }
        public IdentifierNode ClassName { get; set; }
        public KeywordNode AccessModifier { get; set; }
        public ListNode<ParameterNode> Parameters { get; set; }
        public ListNode<AstNode> ThisArgs { get; set; }
        public ListNode<AstNode> Statements { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitConstructor(this);
    }
}

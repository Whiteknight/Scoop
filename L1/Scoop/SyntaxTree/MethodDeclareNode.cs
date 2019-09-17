namespace Scoop.SyntaxTree
{
    public class MethodDeclareNode : AstNode
    {
        // Represents a declaration of a method in an interface
        public ListNode<AttributeNode> Attributes { get; set; }
        public AstNode ReturnType { get; set; }
        public IdentifierNode Name { get; set; }
        public ListNode<ParameterNode> Parameters { get; set; }
        public ListNode<IdentifierNode> GenericTypeParameters { get; set; }
        public ListNode<TypeConstraintNode> TypeConstraints { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitMethodDeclare(this);
    }
}
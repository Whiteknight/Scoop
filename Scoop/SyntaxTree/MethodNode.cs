using Scoop.SyntaxTree.Visiting;

namespace Scoop.SyntaxTree
{
    public class MethodNode : AstNode
    {
        // Represents a single method including access modifier, return type, name, parameters
        // and body
        public ListNode<AttributeNode> Attributes { get; set; }
        public KeywordNode AccessModifier { get; set; }
        public ListNode<KeywordNode> Modifiers { get; set; }
        public AstNode ReturnType { get; set; }
        public IdentifierNode Name { get; set; }
        public ListNode<ParameterNode> Parameters { get; set; }
        public ListNode<TypeConstraintNode> TypeConstraints { get; set; }
        public ListNode<AstNode> Statements { get; set; }
        public ListNode<IdentifierNode> GenericTypeParameters { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitMethod(this);
    }
}
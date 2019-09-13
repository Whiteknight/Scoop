using System.Collections.Generic;

namespace Scoop.SyntaxTree
{
    public class MethodDeclareNode : AstNode, IHasAttributes
    {
        // Represents a declaration of a method in an interface
        public MethodDeclareNode(IReadOnlyList<Diagnostic> diagnostics = null) : base(diagnostics)
        {
        }

        public ListNode<AttributeNode> Attributes { get; set; }
        public AstNode ReturnType { get; set; }
        public IdentifierNode Name { get; set; }
        public ListNode<ParameterNode> Parameters { get; set; }
        public ListNode<IdentifierNode> GenericTypeParameters { get; set; }
        public ListNode<TypeConstraintNode> TypeConstraints { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitMethodDeclare(this);
    }
}
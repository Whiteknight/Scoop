using System.Collections.Generic;

namespace Scoop.SyntaxTree
{
    public class ClassNode : AstNode, IHasAttributes
    {
        // ClassNode contains the class access modifier, class name, inherited interfaces
        // and all the fields/properties/methods/subclasses of the class
        public ClassNode(IReadOnlyList<Diagnostic> diagnostics = null) : base(diagnostics)
        {
        }


        public ListNode<AttributeNode> Attributes { get; set; }

        // "public" or "private"
        public KeywordNode AccessModifier { get; set; }

        // "partial", etc
        public ListNode<KeywordNode> Modifiers { get; set; }

        // "class" or "struct"
        public KeywordNode Type { get; set; }

        public IdentifierNode Name { get; set; }

        public ListNode<IdentifierNode> GenericTypeParameters { get; set; }

        public ListNode<TypeNode> Interfaces { get; set; }
        public ListNode<TypeConstraintNode> TypeConstraints { get; set; }

        // Constructors, methods, fields, properties, child classes, etc
        public ListNode<AstNode> Members { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitClass(this);
    }
}
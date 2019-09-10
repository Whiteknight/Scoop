using System.Collections.Generic;

namespace Scoop.SyntaxTree
{
    public class InterfaceNode : AstNode, IHasAttributes
    {
        // ClassNode contains the class access modifier, class name, inherited interfaces
        // and all the fields/properties/methods/subclasses of the class

        public ListNode<AttributeNode> Attributes { get; set; }
        public KeywordNode AccessModifier { get; set; }
        public IdentifierNode Name { get; set; }

        public ListNode<IdentifierNode> GenericTypeParameters { get; set; }

        public ListNode<TypeNode> Interfaces { get; set; }
        public ListNode<TypeConstraintNode> TypeConstraints { get; set; }

        // Constructors, methods, fields, properties, child classes, etc
        public ListNode<MethodDeclareNode> Members { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitInterface(this);
    }
}
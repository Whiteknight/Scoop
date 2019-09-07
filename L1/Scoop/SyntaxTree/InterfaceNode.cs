using System.Collections.Generic;

namespace Scoop.SyntaxTree
{
    public class InterfaceNode : AstNode, IHasAttributes
    {
        // ClassNode contains the class access modifier, class name, inherited interfaces
        // and all the fields/properties/methods/subclasses of the class

        public List<AttributeNode> Attributes { get; set; }
        public KeywordNode AccessModifier { get; set; }
        public IdentifierNode Name { get; set; }

        public List<AstNode> GenericTypeParameters { get; set; }

        public List<AstNode> Interfaces { get; set; }
        public List<TypeConstraintNode> TypeConstraints { get; set; }

        // Constructors, methods, fields, properties, child classes, etc
        public List<AstNode> Members { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitInterface(this);
    }
}
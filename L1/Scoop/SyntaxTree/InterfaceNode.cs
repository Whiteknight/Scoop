using System.Collections.Generic;

namespace Scoop.SyntaxTree
{
    public class InterfaceNode : AstNode
    {
        // ClassNode contains the class access modifier, class name, inherited interfaces
        // and all the fields/properties/methods/subclasses of the class

        public KeywordNode AccessModifier { get; set; }
        public IdentifierNode Name { get; set; }
        // TODO: This needs to be a better node type to handle things like generics
        public List<DottedIdentifierNode> Interfaces { get; set; }

        // Constructors, methods, fields, properties, child classes, etc
        public List<AstNode> Members { get; set; }

        public override AstNode Accept(AstNodeVisitor visitor) => visitor.VisitInterface(this);
    }
}
using System.Collections.Generic;

namespace Scoop.SyntaxTree
{
    public class DelegateNode : AstNode
    {
        // Represents a delegate declaration

        public List<AttributeNode> Attributes { get; set; }
        public AstNode AccessModifier { get; set; }
        public AstNode ReturnType { get; set; }
        public IdentifierNode Name { get; set; }
        public List<ParameterNode> Parameters { get; set; }
        public List<AstNode> GenericTypeParameters { get; set; }
        public List<TypeConstraintNode> TypeConstraints { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitDelegate(this);
    }
}
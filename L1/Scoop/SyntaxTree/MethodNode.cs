using System.Collections.Generic;

namespace Scoop.SyntaxTree
{
    public class MethodNode : AstNode
    {
        // Represents a single method including access modifier, return type, name, parameters
        // and body

        public KeywordNode AccessModifier { get; set; }
        public List<KeywordNode> Modifiers { get; set; }
        public AstNode ReturnType { get; set; }
        public IdentifierNode Name { get; set; }
        public List<ParameterNode> Parameters { get; set; }
        public List<TypeConstraintNode> TypeConstraints { get; set; }
        public List<AstNode> Statements { get; set; }
        public List<AstNode> GenericTypeParameters { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitMethod(this);

        public void AddModifier(KeywordNode modifier)
        {
            if (Modifiers == null)
                Modifiers = new List<KeywordNode>();
            Modifiers.Add(modifier);
        }
    }

    public class MethodDeclareNode : AstNode
    {
        // Represents a declaration of a method in an interface

        public AstNode ReturnType { get; set; }
        public IdentifierNode Name { get; set; }
        public List<ParameterNode> Parameters { get; set; }
        public List<AstNode> GenericTypeParameters { get; set; }
        public List<TypeConstraintNode> TypeConstraints { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitMethodDeclare(this);
    }
}
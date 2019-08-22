using System;
using System.Collections.Generic;

namespace Scoop.SyntaxTree
{
    public class MethodNode : AstNode
    {
        // Represents a single method including access modifier, return type, name, parameters
        // and body

        public KeywordNode AccessModifier { get; set; }
        public AstNode ReturnType { get; set; }
        public IdentifierNode Name { get; set; }
        public List<AstNode> Parameters { get; set; }
        public List<AstNode> Statements { get; set; }

        public override AstNode Accept(AstNodeVisitor visitor) => visitor.VisitMethod(this);
    }
}
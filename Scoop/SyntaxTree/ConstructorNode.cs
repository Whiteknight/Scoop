using System;
using System.Collections.Generic;

namespace Scoop.SyntaxTree
{
    public class ConstructorNode : AstNode
    {
        // Represents a constructor, a special case of method

        public IdentifierNode ClassName { get; set; }
        public KeywordNode AccessModifier { get; set; }
        public List<AstNode> Parameters { get; set; }
        public List<AstNode> Statements { get; set; }

        public override AstNode Accept(AstNodeVisitor visitor) => visitor.VisitConstructor(this);
    }
}

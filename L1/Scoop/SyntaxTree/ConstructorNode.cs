using System;
using System.Collections.Generic;

namespace Scoop.SyntaxTree
{
    public class ConstructorNode : AstNode
    {
        // Represents a constructor, a special case of method

        public IdentifierNode ClassName { get; set; }
        public KeywordNode AccessModifier { get; set; }
        public List<ParameterNode> Parameters { get; set; }
        public List<AstNode> ThisArgs { get; set; }
        public List<AstNode> Statements { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitConstructor(this);
    }
}

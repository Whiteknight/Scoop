using System.Collections.Generic;

namespace Scoop.SyntaxTree
{
    public class VariableDeclareNode : AstNode
    {
        public VariableDeclareNode(IReadOnlyList<Diagnostic> diagnostics = null) : base(diagnostics)
        {
        }

        public TypeNode Type { get; set; }
        public IdentifierNode Name { get; set; }
        public AstNode Value { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitVariableDeclare(this);
    }
}

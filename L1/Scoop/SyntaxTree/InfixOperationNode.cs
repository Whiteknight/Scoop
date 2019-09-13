using System.Collections.Generic;

namespace Scoop.SyntaxTree
{
    public class InfixOperationNode : AstNode
    {
        public InfixOperationNode(IReadOnlyList<Diagnostic> diagnostics = null) : base(diagnostics)
        {
        }


        public AstNode Left { get; set; }
        public OperatorNode Operator { get; set; }
        public AstNode Right { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitInfixOperation(this);
    }
}
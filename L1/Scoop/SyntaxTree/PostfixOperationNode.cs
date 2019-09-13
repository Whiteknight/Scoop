using System.Collections.Generic;

namespace Scoop.SyntaxTree
{
    public class PostfixOperationNode : AstNode
    {
        public PostfixOperationNode(IReadOnlyList<Diagnostic> diagnostics = null) : base(diagnostics)
        {
        }

        public AstNode Left { get; set; }
        public OperatorNode Operator { get; set; }
        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitPostfixOperation(this);
    }
}
using System.Collections.Generic;

namespace Scoop.SyntaxTree
{
    public class PrefixOperationNode : AstNode
    {
        public PrefixOperationNode(IReadOnlyList<Diagnostic> diagnostics = null) : base(diagnostics)
        {
        }

        public OperatorNode Operator { get; set; }
        public AstNode Right { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitPrefixOperation(this);
    }

    public class CastNode : AstNode
    {
        public CastNode(IReadOnlyList<Diagnostic> diagnostics = null) : base(diagnostics)
        {
        }

        public TypeNode Type { get; set; }
        public AstNode Right { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitCast(this);
    }
}
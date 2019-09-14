using System.Collections.Generic;

namespace Scoop.SyntaxTree
{
    public class EmptyNode : AstNode
    {
        public EmptyNode() : base(null)
        {
        }

        public EmptyNode(IReadOnlyList<Diagnostic> diagnostics = null) : base(diagnostics)
        {
        }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitEmpty(this);
    }
}

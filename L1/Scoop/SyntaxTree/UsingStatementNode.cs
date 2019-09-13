using System.Collections.Generic;

namespace Scoop.SyntaxTree
{
    public class UsingStatementNode : AstNode
    {
        public UsingStatementNode(IReadOnlyList<Diagnostic> diagnostics = null) : base(diagnostics)
        {
        }

        public AstNode Disposable { get; set; }
        public AstNode Statement { get; set; }
        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitUsingStatement(this);
    }
}

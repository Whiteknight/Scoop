using System.Collections.Generic;

namespace Scoop.SyntaxTree
{
    public class NamedArgumentNode : AstNode
    {
        public NamedArgumentNode(IReadOnlyList<Diagnostic> diagnostics = null) : base(diagnostics)
        {
        }

        public IdentifierNode Name { get; set; }
        public OperatorNode Separator { get; set; }
        public AstNode Value { get; set; }
        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitNamedArgument(this);
    }
}

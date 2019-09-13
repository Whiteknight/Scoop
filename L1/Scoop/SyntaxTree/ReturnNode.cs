using System.Collections.Generic;

namespace Scoop.SyntaxTree
{
    public class ReturnNode : AstNode
    {
        public ReturnNode(IReadOnlyList<Diagnostic> diagnostics = null) : base(diagnostics)
        {
        }


        public ReturnNode(AstNode expression, Location location = null, IReadOnlyList<Diagnostic> d = null) : base(d)
        {
            Expression = expression;
            Location = location ?? expression.Location;
        }

        public AstNode Expression { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitReturn(this);
    }
}
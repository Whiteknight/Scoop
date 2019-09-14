using System.Collections.Generic;
using Scoop.Tokenization;

namespace Scoop.SyntaxTree
{
    public class OperatorNode : AstNode
    {
        public OperatorNode() : base(null)
        {
        }

        public OperatorNode(Token t, IReadOnlyList<Diagnostic> d = null) : base(d)
        {
            Operator = t.Value;
            Location = t.Location;
        }

        public OperatorNode(string op, Location location = null, IReadOnlyList<Diagnostic> d = null) : base(d)
        {
            Operator = op;
            Location = location;
        }

        public string Operator { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitOperator(this);
    }
}

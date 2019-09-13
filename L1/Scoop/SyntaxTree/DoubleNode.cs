using System.Collections.Generic;
using Scoop.Tokenization;

namespace Scoop.SyntaxTree
{
    public class DoubleNode : AstNode
    {
        public DoubleNode(double value, IReadOnlyList<Diagnostic> diagnostics = null) : base(diagnostics)
        {
            Value = value;
        }

        public DoubleNode(Token t, IReadOnlyList<Diagnostic> diagnostics = null) : base(diagnostics)
        {
            Value = double.Parse(t.Value);
            Location = t.Location;
        }

        public double Value { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitDouble(this);
    }
}
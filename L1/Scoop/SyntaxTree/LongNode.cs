using System.Collections.Generic;
using Scoop.Tokenization;

namespace Scoop.SyntaxTree
{
    public class LongNode : AstNode
    {
        public LongNode(long value, IReadOnlyList<Diagnostic> diagnostics = null) : base(diagnostics)
        {
            Value = value;
        }

        public LongNode(Token t, IReadOnlyList<Diagnostic> diagnostics = null) : base(diagnostics)
        {
            Value = long.Parse(t.Value);
            Location = t.Location;
        }

        public long Value { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitLong(this);
    }
}
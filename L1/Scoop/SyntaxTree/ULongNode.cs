using System.Collections.Generic;
using Scoop.Tokenization;

namespace Scoop.SyntaxTree
{
    public class ULongNode : AstNode
    {
        public ULongNode(ulong value, IReadOnlyList<Diagnostic> d = null) : base(d)
        {
            Value = value;
        }

        public ULongNode(Token t, IReadOnlyList<Diagnostic> d = null) : base(d)
        {
            Value = ulong.Parse(t.Value);
            Location = t.Location;
        }

        public ulong Value { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitULong(this);
    }
}
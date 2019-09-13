using System.Collections.Generic;
using Scoop.Tokenization;

namespace Scoop.SyntaxTree
{
    public class UIntegerNode : AstNode
    {
        public UIntegerNode(uint value, IReadOnlyList<Diagnostic> d = null) : base(d)
        {
            Value = value;
        }

        public UIntegerNode(Token t, IReadOnlyList<Diagnostic> d = null) : base(d)
        {
            Value = uint.Parse(t.Value);
            Location = t.Location;
        }

        public uint Value { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitUInteger(this);
    }
}
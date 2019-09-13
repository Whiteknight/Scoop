using System.Collections.Generic;
using Scoop.Tokenization;

namespace Scoop.SyntaxTree
{
    public class IntegerNode : AstNode
    {
        public IntegerNode(int value, IReadOnlyList<Diagnostic> diagnostics = null) : base(diagnostics)
        {
            Value = value;
        }

        public IntegerNode(Token t, IReadOnlyList<Diagnostic> diagnostics = null) : base(diagnostics)
        {
            Value = int.Parse(t.Value);
            Location = t.Location;
        }

        public int Value { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitInteger(this);
    }
}
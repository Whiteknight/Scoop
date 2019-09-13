using System.Collections.Generic;
using Scoop.Tokenization;

namespace Scoop.SyntaxTree
{
    public class FloatNode : AstNode
    {
        public FloatNode(float value, IReadOnlyList<Diagnostic> diagnostics = null) : base(diagnostics)
        {
            Value = value;
        }

        public FloatNode(Token t, IReadOnlyList<Diagnostic> diagnostics = null) : base(diagnostics)
        {
            Value = float.Parse(t.Value);
            Location = t.Location;
        }

        public float Value { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitFloat(this);
    }
}
using System.Collections.Generic;
using Scoop.Tokenization;

namespace Scoop.SyntaxTree
{
    public class CharNode : AstNode
    {
        public CharNode(char value, Location location = null, IReadOnlyList<Diagnostic> diagnostics = null) : base(diagnostics)
        {
            Value = value;
            Location = location;
        }

        public CharNode(Token t, IReadOnlyList<Diagnostic> diagnostics = null) : base(diagnostics)
        {
            Value = t.Value[0];
            Location = t.Location;
        }

        public char Value { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitChar(this);
    }
}
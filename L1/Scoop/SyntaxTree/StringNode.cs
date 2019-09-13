using System.Collections.Generic;
using Scoop.Tokenization;

namespace Scoop.SyntaxTree
{
    public class StringNode : AstNode
    {
        public StringNode(string value, Location location = null, IReadOnlyList<Diagnostic> d = null) : base(d)
        {
            Value = value;
            Location = location;
        }

        public StringNode(Token t, IReadOnlyList<Diagnostic> d = null) : base(d)
        {
            Value = t.Value;
            Location = t.Location;
        }

        public string Value { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitString(this);
    }
}
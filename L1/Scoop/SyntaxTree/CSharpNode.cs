using System.Collections.Generic;
using Scoop.Tokenization;

namespace Scoop.SyntaxTree
{
    public class CSharpNode : AstNode
    {
        public CSharpNode(IReadOnlyList<Diagnostic> diagnostics = null) : base(diagnostics)
        {
        }


        public CSharpNode(string code, IReadOnlyList<Diagnostic> diagnostics = null) : base(diagnostics)
        {
            Code = code;
        }

        public CSharpNode(Token t, IReadOnlyList<Diagnostic> diagnostics = null) : base(diagnostics)
        {
            Location = t.Location;
            Code = t.Value;
        }

        public string Code { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitCSharp(this);
    }
}

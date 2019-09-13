using System;
using System.Collections.Generic;

namespace Scoop.SyntaxTree
{
    public class UsingDirectiveNode : AstNode
    {
        // Represents a using directive, which imports namespace symbols into the current
        // compilation unit
        public UsingDirectiveNode(IReadOnlyList<Diagnostic> diagnostics = null) : base(diagnostics)
        {
        }

        public IdentifierNode Alias { get; set; }
        public DottedIdentifierNode Namespace { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitUsingDirective(this);
    }
}
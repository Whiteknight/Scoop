using System;

namespace Scoop.SyntaxTree
{
    public class UsingDirectiveNode : AstNode
    {
        // Represents a using directive, which imports namespace symbols into the current
        // compilation unit

        public IdentifierNode Alias { get; set; }
        public DottedIdentifierNode Namespace { get; set; }

        public override AstNode Accept(AstNodeVisitor visitor) => visitor.VisitUsingDirective(this);
    }
}
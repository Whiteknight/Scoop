using System.Collections.Generic;

namespace Scoop.SyntaxTree
{
    public class FieldNode : AstNode, IHasAttributes
    {
        public FieldNode(IReadOnlyList<Diagnostic> diagnostics = null) : base(diagnostics)
        {
        }


        public ListNode<AttributeNode> Attributes { get; set; }
        public TypeNode Type { get; set; }
        public IdentifierNode Name { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitField(this);
    }
}

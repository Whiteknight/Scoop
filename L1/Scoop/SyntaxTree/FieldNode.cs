using System.Collections.Generic;

namespace Scoop.SyntaxTree
{
    public class FieldNode : AstNode, IHasAttributes
    {
        public List<AttributeNode> Attributes { get; set; }
        public AstNode Type { get; set; }
        public IdentifierNode Name { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitField(this);
    }
}

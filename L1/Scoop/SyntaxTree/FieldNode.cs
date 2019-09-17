namespace Scoop.SyntaxTree
{
    public class FieldNode : AstNode
    {
        public ListNode<AttributeNode> Attributes { get; set; }
        public TypeNode Type { get; set; }
        public IdentifierNode Name { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitField(this);
    }
}

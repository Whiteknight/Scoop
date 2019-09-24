using Scoop.SyntaxTree.Visiting;

namespace Scoop.SyntaxTree
{
    public class TypeNode : AstNode
    {
        public TypeNode()
        {
        }

        public TypeNode(string typeName)
        {
            Name = new IdentifierNode(typeName);
        }

        public TypeNode(IdentifierNode id)
        {
            Name = id;
            Location = id.Location;
        }

        public IdentifierNode Name { get; set; }
        public ListNode<TypeNode> GenericArguments { get; set; }
        public ListNode<ArrayTypeNode> ArrayTypes { get; set; }
        public TypeNode Child { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitType(this);
    }

    public class ArrayTypeNode : AstNode
    {
        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitArrayType(this);
    }
}

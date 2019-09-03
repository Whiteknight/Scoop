using System.Collections.Generic;

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

        public IdentifierNode Name { get; set; }
        public List<AstNode> GenericArguments { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitType(this);
    }

    public class ChildTypeNode : AstNode
    {
        public AstNode Parent { get; set; }
        public TypeNode Child { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitChildType(this);
    }

    public class ArrayTypeNode : AstNode
    {
        public AstNode ElementType { get; set; }
        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitArrayType(this);
    }
}

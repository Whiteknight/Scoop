using System;
using System.Collections.Generic;
using System.Text;

namespace Scoop.SyntaxTree
{
    public class TypeNode : AstNode
    {
        public IdentifierNode Name { get; set; }
        public List<AstNode> GenericArguments { get; set; }

        public override AstNode Accept(AstNodeVisitor visitor) => visitor.VisitType(this);
    }

    public class ChildTypeNode : AstNode
    {
        public AstNode Parent { get; set; }
        public TypeNode Child { get; set; }

        public override AstNode Accept(AstNodeVisitor visitor) => visitor.VisitChildType(this);
    }

    public class ArrayTypeNode : AstNode
    {
        public AstNode ElementType { get; set; }
        public override AstNode Accept(AstNodeVisitor visitor) => visitor.VisitArrayType(this);
    }
}

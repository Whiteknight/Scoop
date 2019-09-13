using System.Collections.Generic;

namespace Scoop.SyntaxTree
{
    public class TypeNode : AstNode
    {
        public TypeNode(IReadOnlyList<Diagnostic> diagnostics = null) : base(diagnostics)
        {
        }

        public TypeNode(string typeName, IReadOnlyList<Diagnostic> diagnostics = null) : base(diagnostics)
        {
            Name = new IdentifierNode(typeName);
        }

        public TypeNode(IdentifierNode id, IReadOnlyList<Diagnostic> diagnostics = null) : base(diagnostics)
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
        public ArrayTypeNode(IReadOnlyList<Diagnostic> diagnostics = null) : base(diagnostics)
        {
        }
        // TODO: N-arity
        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitArrayType(this);
    }
}

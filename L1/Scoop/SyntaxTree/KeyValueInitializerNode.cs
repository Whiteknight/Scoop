using System.Collections.Generic;

namespace Scoop.SyntaxTree
{
    public class KeyValueInitializerNode : AstNode
    {
        public KeyValueInitializerNode(IReadOnlyList<Diagnostic> diagnostics = null) : base(diagnostics)
        {
        }

        public AstNode Key { get; set; }
        public AstNode Value { get; set; }
        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitKeyValueInitializer(this);
    }

    public class ArrayInitializerNode : AstNode
    {
        public ArrayInitializerNode(IReadOnlyList<Diagnostic> diagnostics = null) : base(diagnostics)
        {
        }

        public IntegerNode Key { get; set; }
        public AstNode Value { get; set; }
        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitArrayInitializer(this);
    }

    public class PropertyInitializerNode : AstNode
    {
        public PropertyInitializerNode(IReadOnlyList<Diagnostic> diagnostics = null) : base(diagnostics)
        {
        }

        public IdentifierNode Property { get; set; }
        public AstNode Value { get; set; }
        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitPropertyInitializer(this);
    }
}

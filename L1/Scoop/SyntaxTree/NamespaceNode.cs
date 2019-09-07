using System.Collections.Generic;

namespace Scoop.SyntaxTree
{
    public class NamespaceNode : AstNode, IHasAttributes
    {
        public List<AttributeNode> Attributes { get; set; }

        public DottedIdentifierNode Name { get; set; }
        // classes, interfaces, etc
        public List<AstNode> Declarations { get; set; }

        public void AddDeclaration(AstNode declaration)
        {
            if (Declarations == null)
                Declarations = new List<AstNode>();
            Declarations.Add(declaration);
        }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitNamespace(this);
    }
}
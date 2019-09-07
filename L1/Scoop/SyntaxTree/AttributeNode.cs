using System.Collections.Generic;

namespace Scoop.SyntaxTree
{
    public class AttributeNode : AstNode
    {
        public KeywordNode Target { get; set; }
        public AstNode Type { get; set; }
        public List<AstNode> Arguments { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitAttribute(this);
    }
}

using System.Collections.Generic;

namespace Scoop.SyntaxTree
{
    public class NewNode : AstNode
    {
        public AstNode Type { get; set; }
        public List<AstNode> Arguments { get; set; }

        public override AstNode Accept(AstNodeVisitor visitor) => visitor.VisitNew(this);
    }
}
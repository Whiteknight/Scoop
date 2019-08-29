using System.Collections.Generic;

namespace Scoop.SyntaxTree
{
    public class NewNode : AstNode
    {
        public AstNode Type { get; set; }
        public List<AstNode> Arguments { get; set; }
        public List<AstNode> Initializers { get; set; }
        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitNew(this);
    }
}
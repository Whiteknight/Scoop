using System.Collections.Generic;

namespace Scoop.SyntaxTree
{
    public class LambdaNode : AstNode
    {
        public List<AstNode> Parameters { get; set; }
        public List<AstNode> Statements { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitLambda(this);
    }
}

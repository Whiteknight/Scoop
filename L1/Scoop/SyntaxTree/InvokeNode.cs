using System;
using System.Collections.Generic;
using System.Text;

namespace Scoop.SyntaxTree
{
    public class InvokeNode : AstNode
    {
        public AstNode Instance { get; set; }
        public List<AstNode> Arguments { get; set; }

        public override AstNode Accept(AstNodeVisitor visitor) => visitor.VisitInvoke(this);
    }
}

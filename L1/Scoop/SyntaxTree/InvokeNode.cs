﻿using System.Collections.Generic;

namespace Scoop.SyntaxTree
{
    public class InvokeNode : AstNode
    {
        public AstNode Instance { get; set; }
        public List<AstNode> Arguments { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitInvoke(this);
    }
}

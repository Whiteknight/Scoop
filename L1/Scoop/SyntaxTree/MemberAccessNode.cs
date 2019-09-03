﻿using System.Collections.Generic;

namespace Scoop.SyntaxTree
{
    public class MemberAccessNode : AstNode
    {
        public AstNode Instance { get; set; }
        public IdentifierNode MemberName { get; set; }
        public bool IgnoreNulls { get; set; }
        public List<AstNode> GenericArguments { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitMemberAccess(this);

    }
}
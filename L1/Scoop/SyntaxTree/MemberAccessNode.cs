﻿namespace Scoop.SyntaxTree
{
    public class MemberAccessNode : AstNode
    {
        public AstNode Instance { get; set; }
        public IdentifierNode MemberName { get; set; }

        public override AstNode Accept(AstNodeVisitor visitor) => visitor.VisitMemberAccess(this);

    }
}
namespace Scoop.SyntaxTree
{
    public class MemberAccessNode : AstNode
    {
        public AstNode Instance { get; set; }
        public IdentifierNode MemberName { get; set; }
        public bool IgnoreNulls { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitMemberAccess(this);

    }
}
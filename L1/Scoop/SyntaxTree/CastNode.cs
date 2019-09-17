namespace Scoop.SyntaxTree
{
    public class CastNode : AstNode
    {
        public TypeNode Type { get; set; }
        public AstNode Right { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitCast(this);
    }
}
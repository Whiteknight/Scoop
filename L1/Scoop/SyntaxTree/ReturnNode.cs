namespace Scoop.SyntaxTree
{
    public class ReturnNode : AstNode
    {
        public ReturnNode()
        {
        }

        public ReturnNode(AstNode expression, Location location = null)
        {
            Expression = expression;
            Location = location ?? expression.Location;
        }

        public AstNode Expression;

        public override AstNode Accept(AstNodeVisitor visitor) => visitor.VisitReturn(this);
    }
}
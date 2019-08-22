namespace Scoop.SyntaxTree
{
    public class ParenthesisNode<TNode> : AstNode
        where TNode : AstNode
    {
        public ParenthesisNode(Location location = null)
        {
            Location = location;
        }

        public ParenthesisNode(TNode expression, Location location = null)
        {
            Expression = expression;
            Location = location ?? expression.Location;
        }

        public TNode Expression { get; set; }
        public override AstNode Accept(AstNodeVisitor visitor) => visitor.VisitParenthesis(this);
    }
}
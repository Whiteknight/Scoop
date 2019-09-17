namespace Scoop.SyntaxTree
{
    public class UsingStatementNode : AstNode
    {
        public AstNode Disposable { get; set; }
        public AstNode Statement { get; set; }
        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitUsingStatement(this);
    }
}

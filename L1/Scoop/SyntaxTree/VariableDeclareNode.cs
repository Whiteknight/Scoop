namespace Scoop.SyntaxTree
{
    public class VariableDeclareNode : AstNode
    {
        // TODO: We only support "var" now for ease of parsing
        //public IdentifierNode Type { get; set; }
        public IdentifierNode Name { get; set; }
        public override AstNode Accept(AstNodeVisitor visitor) => visitor.VisitVariableDeclare(this);
    }
}

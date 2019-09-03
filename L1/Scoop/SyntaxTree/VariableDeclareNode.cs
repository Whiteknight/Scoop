namespace Scoop.SyntaxTree
{
    public class VariableDeclareNode : AstNode
    {
        // TODO: We only support "var" now for ease of parsing
        // TODO: To fix this without the parser getting huge, we might need new syntax
        //public IdentifierNode Type { get; set; }
        public IdentifierNode Name { get; set; }
        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitVariableDeclare(this);
    }
}

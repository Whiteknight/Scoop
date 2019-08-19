namespace Scoop.SyntaxTree
{
    public abstract class AstNode
    {
        public abstract AstNode Accept(AstNodeVisitor visitor);

        //public override string ToString()
        //{
        //    var sb = new StringBuilder();
        //    var visitor = new SqlServerStringifyVisitor(sb);
        //    visitor.Visit(this);
        //    return sb.ToString();
        //}

        public Location Location { get; set; }
    }
}

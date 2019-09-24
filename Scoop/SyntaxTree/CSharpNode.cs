using Scoop.SyntaxTree.Visiting;
using Scoop.Tokenization;

namespace Scoop.SyntaxTree
{
    public class CSharpNode : AstNode
    {
        public CSharpNode(Token t)
        {
            Location = t.Location;
            Code = t.Value;
        }

        public string Code { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitCSharp(this);
    }
}

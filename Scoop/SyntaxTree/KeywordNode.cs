using Scoop.Tokenization;

namespace Scoop.SyntaxTree
{
    public class KeywordNode : AstNode
    {
        public KeywordNode()
        {
        }

        public KeywordNode(Token t)
        {
            Keyword = t.Value;
            Location = t.Location;
        }

        public KeywordNode(string keyword)
        {
            Keyword = keyword;
        }

        public string Keyword { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitKeyword(this);
    }
}
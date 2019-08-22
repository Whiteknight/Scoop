using Scoop.Tokenization;

namespace Scoop.SyntaxTree
{
    public class KeywordNode : AstNode
    {
        public KeywordNode(Token t)
        {
            if (t.Type != TokenType.Keyword)
                throw ParsingException.UnexpectedToken(TokenType.Keyword, t);
            Keyword = t.Value;
            Location = t.Location;
        }

        public KeywordNode(string keyword)
        {
            Keyword = keyword;
        }

        public string Keyword { get; }

        public override AstNode Accept(AstNodeVisitor visitor) => visitor.VisitKeyword(this);
    }
}
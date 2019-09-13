using System.Collections.Generic;
using Scoop.Tokenization;

namespace Scoop.SyntaxTree
{
    public class KeywordNode : AstNode
    {
        public KeywordNode(IReadOnlyList<Diagnostic> diagnostics = null) : base(diagnostics)
        {
        }

        public KeywordNode(Token t, IReadOnlyList<Diagnostic> d = null) : base(d)
        {
            if (t.Type != TokenType.Keyword)
                throw ParsingException.UnexpectedToken(TokenType.Keyword, t);
            Keyword = t.Value;
            Location = t.Location;
        }

        public KeywordNode(string keyword, IReadOnlyList<Diagnostic> d = null) : base(d)
        {
            Keyword = keyword;
        }

        public string Keyword { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitKeyword(this);
    }
}
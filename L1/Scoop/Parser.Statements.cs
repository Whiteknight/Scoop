using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop
{
    public partial class Parser
    {
        private AstNode ParseStatement(Tokenizer t)
        {
            var stmt = ParseStatement0(t);
            if (stmt == null)
                return null;
            t.Expect(TokenType.Operator, ";");
            return stmt;
        }

        private AstNode ParseStatement0(Tokenizer t)
        {
            var lookahead = t.Peek();
            if (lookahead.IsKeyword("return"))
                return ParseReturn(t);

            return null;
        }

        private ReturnNode ParseReturn(Tokenizer t)
        {
            var returnToken = t.Expect(TokenType.Keyword, "return");
            return new ReturnNode
            {
                Location = returnToken.Location,
                Expression = ParseExpression(t)
            };
        }
    }
}

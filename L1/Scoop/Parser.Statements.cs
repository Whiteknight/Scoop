using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop
{
    public partial class Parser
    {
        // Helper for testing
        public AstNode ParseStatement(string s) => ParseStatement(new Tokenizer(new StringCharacterSequence(s)));

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
            if (lookahead.Is(TokenType.Operator, "}"))
                return null;
            if (lookahead.IsKeyword("return"))
                return ParseReturn(t);
            if (lookahead.IsKeyword("var"))
                return ParseDeclaration(t);
            // TODO: Using statement

            return ParseExpression(t);
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

        private AstNode ParseDeclaration(Tokenizer t)
        {
            var varToken = t.Expect(TokenType.Keyword, "var");
            var nameToken = t.Expect(TokenType.Identifier);
            var declareNode = new VariableDeclareNode
            {
                Location = varToken.Location,
                Name = new IdentifierNode(nameToken)
            };

            var lookahead = t.Peek();
            if (!lookahead.Is(TokenType.Operator, "="))
                return declareNode;

            var assignmentToken = t.GetNext();
            var expr = ParseExpression(t);
            return new InfixOperationNode
            {
                Location = declareNode.Location,
                Left = declareNode,
                Operator = new OperatorNode(assignmentToken),
                Right = expr
            };
        }
    }
}

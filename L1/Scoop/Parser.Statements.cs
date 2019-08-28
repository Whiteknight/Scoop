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
            // Skip over any bare semicolons, which indicate an empty statement.
            while (t.Peek().IsOperator(";"))
                t.Advance();

            var lookahead = t.Peek();
            if (lookahead.IsType(TokenType.CSharpLiteral))
                return new CSharpNode(t.GetNext());

            var stmt = ParseStatementUnterminated(t);
            if (stmt == null)
                return null;
            t.Expect(TokenType.Operator, ";");
            return stmt;
        }

        private AstNode ParseStatementUnterminated(Tokenizer t)
        {
            var lookahead = t.Peek();
            if (lookahead.Is(TokenType.Operator, "}"))
                // TODO: Would be nice to return an End-Block statement instead of null
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

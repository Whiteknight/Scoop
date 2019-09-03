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

            // Parse using-statement. This already includes it's own ";"
            if (lookahead.IsKeyword("using"))
                return ParseUsingStatement(t);

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
                return null;
            if (lookahead.IsKeyword("return"))
                return ParseReturn(t);
            if (lookahead.IsKeyword("var"))
                return ParseDeclaration(t);

            return ParseExpression(t);
        }

        private UsingStatementNode ParseUsingStatement(Tokenizer t)
        {
            // TODO: "using" "(" <nonAssignmentExpression> ")"
            var usingToken = t.Expect(TokenType.Keyword, "using");
            t.Expect(TokenType.Operator, "(");
            var declareToken = t.Expect(TokenType.Keyword, "var");
            var variable = t.Expect(TokenType.Identifier);
            var assign = t.Expect(TokenType.Operator, "=");
            var source = ParseExpressionConditional(t);
            t.Expect(TokenType.Operator, ")");
            // TODO: Multiple statements in a "{" "}" block?
            var statement = ParseStatement(t);
            return new UsingStatementNode
            {
                Location = usingToken.Location,
                Disposable = new InfixOperationNode
                {
                    Location = declareToken.Location,
                    Left = new VariableDeclareNode
                    {
                        Location = declareToken.Location,
                        Name = new IdentifierNode(variable)
                    },
                    Operator = new OperatorNode(assign),
                    Right = source
                },
                Statement = statement
            };
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

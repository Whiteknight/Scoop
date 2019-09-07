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
            // <csharpLiteral> | <usingStatement> | (<unterminatedStatement> ";") | null
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
            // "using" "(" "var" <ident> "=" <expression> ")" <statement>
            // "using" "(" <expression> ")" <statement>
            var usingToken = t.Expect(TokenType.Keyword, "using");
            t.Expect(TokenType.Operator, "(");
            var lookaheads = t.Peek(2);
            var usingNode = new UsingStatementNode
            {
                Location = usingToken.Location
            };
            if (lookaheads[0].IsKeyword("var") && lookaheads[1].IsType(TokenType.Identifier))
            {
                var declareToken = t.Expect(TokenType.Keyword, "var");
                var variable = t.Expect(TokenType.Identifier);
                var assign = t.Expect(TokenType.Operator, "=");
                var source = ParseExpressionConditional(t);
                usingNode.Disposable = new InfixOperationNode
                {
                    Location = declareToken.Location,
                    Left = new VariableDeclareNode
                    {
                        Location = declareToken.Location,
                        Name = new IdentifierNode(variable)
                    },
                    Operator = new OperatorNode(assign),
                    Right = source
                };
            }
            else
                usingNode.Disposable = ParseExpressionNonComma(t);
            t.Expect(TokenType.Operator, ")");
            var statement = ParseStatement(t);
            usingNode.Statement = statement;
            return usingNode;
        }

        private ReturnNode ParseReturn(Tokenizer t)
        {
            // "return" <expression>
            var returnToken = t.Expect(TokenType.Keyword, "return");
            return new ReturnNode
            {
                Location = returnToken.Location,
                Expression = ParseExpression(t)
            };
        }

        private AstNode ParseDeclaration(Tokenizer t)
        {
            // "var" <ident> ("=" <expression>)?
            var varToken = t.Expect(TokenType.Keyword, "var");
            var nameToken = t.Expect(TokenType.Identifier);
            var declareNode = new VariableDeclareNode
            {
                Location = varToken.Location,
                Name = new IdentifierNode(nameToken)
            };

            if (!t.Peek().Is(TokenType.Operator, "="))
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

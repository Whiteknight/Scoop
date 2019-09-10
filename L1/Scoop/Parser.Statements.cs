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
            // <csharpLiteral> | <usingStatement> | <unterminatedStatement> ";" | null
            // Skip over any bare semicolons, which indicate an empty statement.
            while (t.Peek().IsOperator(";"))
                t.Advance();

            if (t.Peek().IsType(TokenType.CSharpLiteral))
                return new CSharpNode(t.GetNext());

            // Parse using-statement. This already includes it's own ";"
            if (t.Peek().IsKeyword("using"))
                return ParseUsingStatement(t);

            var stmt = ParseStatementUnterminated(t);
            t.Expect(TokenType.Operator, ";");
            return stmt;
        }

        private AstNode ParseStatementUnterminated(Tokenizer t)
        {
            // <returnStatement | <declaration> | <constDeclaration> | <expression>
            var lookahead = t.Peek();
            if (lookahead.IsKeyword("return"))
                return ParseReturn(t);
            if (lookahead.IsKeyword("var"))
                return ParseDeclaration(t);
            if (lookahead.IsKeyword("const"))
                return ParseConstDeclaration(t);

            return ParseExpression(t);
        }

        private UsingStatementNode ParseUsingStatement(Tokenizer t)
        {
            // "using" "(" "var" <ident> "=" <expression> ")" <statement>
            // "using" "(" <expression> ")" <statement>
            return new UsingStatementNode
            {
                Location = t.Expect(TokenType.Keyword, "using").Location,
                Disposable = ParseUsingDisposable(t),
                Statement = ParseStatement(t)
            };
        }

        private AstNode ParseUsingDisposable(Tokenizer t)
        {
            t.Expect(TokenType.Operator, "(");
            AstNode disposable;
            var lookaheads = t.Peek(2);
            if (lookaheads[0].IsKeyword("var") && lookaheads[1].IsType(TokenType.Identifier))
            {
                var declareToken = t.Expect(TokenType.Keyword, "var");
                disposable = new InfixOperationNode
                {
                    Location = declareToken.Location,
                    Left = new VariableDeclareNode
                    {
                        Location = declareToken.Location,
                        Name = new IdentifierNode(t.Expect(TokenType.Identifier))
                    },
                    Operator = new OperatorNode(t.Expect(TokenType.Operator, "=")),
                    Right = ParseExpressionConditional(t)
                };
            }
            else
                disposable = ParseExpression(t);

            t.Expect(TokenType.Operator, ")");
            return disposable;
        }

        private ReturnNode ParseReturn(Tokenizer t)
        {
            // "return" <expression>
            return new ReturnNode
            {
                Location = t.Expect(TokenType.Keyword, "return").Location,
                // Parse expression. It may be a tuple literal, but those will be surrounded with parens
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
            var expr = ParseExpressionList(t);
            return new InfixOperationNode
            {
                Location = declareNode.Location,
                Left = declareNode,
                Operator = new OperatorNode(assignmentToken),
                Right = expr
            };
        }

        private AstNode ParseConstDeclaration(Tokenizer t)
        {
            // "const" ("var" | <type>) <ident> "=" <expression>
            var constNode = new ConstNode
            {
                Location = t.Expect(TokenType.Keyword, "const").Location,
                Type = t.Peek().IsKeyword("var") ? new TypeNode(t.GetNext().Value) : ParseType(t),
                Name = new IdentifierNode(t.Expect(TokenType.Identifier))
            };
            t.Expect(TokenType.Operator, "=");
            constNode.Value = ParseExpression(t);
            return constNode;
        }
    }
}

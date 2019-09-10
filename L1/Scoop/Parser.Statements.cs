using Scoop.Parsers;
using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop
{
    public partial class Parser
    {
        private void InitializeStatements()
        {
            // "const" ("var" | <type>) <ident> "=" <expression>
            var constParser = ScoopParsers.Sequence(
                new KeywordParser("const"),
                DeclareTypes,
                new IdentifierParser(),
                ScoopParsers.Optional(
                    ScoopParsers.Sequence(
                        new OperatorParser("="),
                        Expressions,
                        (op, expr) => expr
                    )
                ),
                new OperatorParser(";"),
                ProduceConstant
            );
            var varDeclareParser = ScoopParsers.Sequence(
                // <type> <ident> ("=" <expression>)?
                DeclareTypes,
                new IdentifierParser(),
                ScoopParsers.Optional(
                    ScoopParsers.Sequence(
                        new OperatorParser("="),
                        Expressions,
                        (op, expr) => expr
                    )
                ),
                ProduceVariableDeclare
            );
            var varDeclareStmtParser = ScoopParsers.Sequence(
                varDeclareParser,
                new OperatorParser(";"),
                (v, s) => v
            );

            var returnParser = ScoopParsers.Sequence(
                // "return" <expression>
                new KeywordParser("return"),
                Expressions,
                new OperatorParser(";"),
                (r, expr, s) => new ReturnNode
                {
                    Location = r.Location,
                    // Parse expression. It may be a tuple literal, but those will be surrounded with parens
                    Expression = expr
                }
            );
            var usingStmtParser = ScoopParsers.Sequence(
                // "using" "(" "var" <ident> "=" <expression> ")" <statement>
                // "using" "(" <expression> ")" <statement>
                new KeywordParser("using"),
                new OperatorParser("("),
                ScoopParsers.First(
                    varDeclareParser,
                    Expressions
                ),
                new OperatorParser(")"),
                ScoopParsers.Deferred(() => Statements),
                (u, a, disposable, b, stmt) => new UsingStatementNode {
                    Location = u.Location,
                    Disposable = disposable,
                    Statement = stmt
                }
            );

            Statements = ScoopParsers.First(
                // <returnStatement | <declaration> | <constDeclaration> | <expression>
                // <csharpLiteral> | <usingStatement> | <unterminatedStatement> ";" | null
                ScoopParsers.Transform(
                    new OperatorParser(";"),
                    o => new EmptyNode()
                ),
                ScoopParsers.Token(TokenType.CSharpLiteral, x => new CSharpNode(x)),
                usingStmtParser,
                returnParser,
                constParser,
                varDeclareStmtParser,
                ScoopParsers.Sequence(
                    Expressions,
                    new OperatorParser(";"),
                    (expr, s) => expr
                )
            );
        }

        private VariableDeclareNode ProduceVariableDeclare(TypeNode type, IdentifierNode name, AstNode value)
        {
            return new VariableDeclareNode
            {
                Location = type.Location,
                Type = type,
                Name = name,
                Value = value is EmptyNode ? null : value
            };
        }

        private ConstNode ProduceConstant(KeywordNode c, TypeNode type, IdentifierNode name, AstNode value, OperatorNode s)
        {
            return new ConstNode
            {
                Location = c.Location, Type = type, Name = name, Value = value is EmptyNode ? null : value
            };
        }

        // Helper for testing
        public AstNode ParseStatement(string s) => Statements.Parse(new Tokenizer(s)).GetResult();
    }
}

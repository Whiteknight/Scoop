using Scoop.Parsers;
using Scoop.SyntaxTree;
using Scoop.Tokenization;
using static Scoop.Parsers.ScoopParsers;

namespace Scoop.Grammar
{
    public partial class ScoopGrammar
    {
        private void InitializeStatements()
        {
            var constStmtParser = Sequence(
                // "const" <type> <ident> "=" <expression> ";"
                new KeywordParser("const"),
                _requiredType,
                _requiredIdentifier,
                _requiredEquals,
                _requiredExpression,
                _requiredSemicolon,
                (c, type, name, e, expr, s) => new ConstNode {
                    Location = c.Location,
                    Type = type,
                    Name = name,
                    Value = expr
                }.WithUnused(c, e, s)
            ).Named("constStmt");

            var varDeclareParser = Sequence(
                // <type> <ident> ("=" <expression>)? ";"
                DeclareTypes,
                new IdentifierParser(),
                Optional(
                    Sequence(
                        new OperatorParser("="),
                        Expressions,
                        (op, expr) => expr.WithUnused(op)
                    )
                ),
                (type, name, value) => new VariableDeclareNode
                {
                    Location = type.Location,
                    Type = type,
                    Name = name,
                    Value = value is EmptyNode ? null : value
                }
            ).Named("varDeclare");
            var varDeclareStmtParser = Sequence(
                varDeclareParser,
                _requiredSemicolon,
                (v, s) => v.WithUnused(s)
            ).Named("varDeclareStmt");

            var returnStmtParser = Sequence(
                // "return" <expression>? ";"
                new KeywordParser("return"),
                Optional(Expressions),
                _requiredSemicolon,
                (r, expr, s) => new ReturnNode
                {
                    Location = r.Location,
                    Expression = expr is EmptyNode ? null : expr
                }.WithUnused(s)
            ).Named("returnStmt");

            var usingStmtParser = Sequence(
                // "using" "(" <varDeclare> | <expr> ")" <statement>
                new KeywordParser("using"),
                _requiredOpenParen,
                First(
                    varDeclareParser,
                    Expressions,
                    Error<EmptyNode>(false, Errors.MissingExpression)
                ),
                _requiredCloseParen,
                Required(Deferred(() => Statements), () => new EmptyNode(), Errors.MissingStatement),
                (u, a, disposable, b, stmt) => new UsingStatementNode {
                    Location = u.Location,
                    Disposable = disposable,
                    Statement = stmt
                }.WithUnused(a, b)
            ).Named("usingStmt");

            Statements = First(
                // ";" | <returnStatement> | <declaration> | <constDeclaration> | <expression>
                // <csharpLiteral> | <usingStatement>
                Transform(
                    new OperatorParser(";"),
                    o => new EmptyNode().WithUnused(o)
                ).Named("emptyStmt"),
                Token(TokenType.CSharpLiteral, x => new CSharpNode(x)),
                usingStmtParser,
                returnStmtParser,
                constStmtParser,
                varDeclareStmtParser,
                Sequence(
                    Expressions,
                    _requiredSemicolon,
                    (expr, s) => expr.WithUnused(s)
                ).Named("expressionStmt")
            ).Named("Statements");
        }
    }
}

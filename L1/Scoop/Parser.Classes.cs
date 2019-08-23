using System.Collections.Generic;
using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop
{
    public partial class Parser
    {
        // Helper method to start parsing at the class level, mostly to simplify unit tests
        public ClassNode ParseClass(string s) => ParseClass(new Tokenizer(new StringCharacterSequence(s)));

        private ClassNode ParseClass(Tokenizer t)
        {
            var accessModifierToken = t.Expect(TokenType.Keyword, "public", "private");
            t.Expect(TokenType.Keyword, "class");
            var classNameToken = t.Expect(TokenType.Identifier);
            // TODO: ':' <ContractList>
            t.Expect(TokenType.Operator, "{");
            var memberNodes = ParseClassBody(t);
            t.Expect(TokenType.Operator, "}");
            return new ClassNode
            {
                AccessModifier = new KeywordNode(accessModifierToken),
                Name = new IdentifierNode(classNameToken),
                Members = memberNodes
            };
        }

        private List<AstNode> ParseClassBody(Tokenizer t)
        {
            var members = new List<AstNode>();
            while (true)
            {
                var lookahead = t.GetNext();
                if (lookahead.Is(TokenType.Operator, "}"))
                {
                    t.PutBack(lookahead);
                    break;
                }

                if (lookahead.IsKeyword("public", "private"))
                {
                    var lookahead2 = t.Peek();
                    if (lookahead2.IsKeyword("class"))
                    {
                        // TODO: Parse nested child class
                        throw ParsingException.CouldNotParseRule(nameof(ParseClassBody), lookahead2);
                    }

                    t.PutBack(lookahead);
                    var member = ParseConstructorOrMethod(t);
                    members.Add(member);
                    continue;
                }

                throw ParsingException.CouldNotParseRule(nameof(ParseClassBody), lookahead);
            }

            return members;
        }

        public AstNode ParseConstructorOrMethod(string s) => ParseConstructorOrMethod(new Tokenizer(new StringCharacterSequence(s)));

        private AstNode ParseConstructorOrMethod(Tokenizer t)
        {
            var accessModifier = t.Expect(TokenType.Keyword, "public", "private");
            var returnType = ParseType(t);
            AstNode node = null;
            if (t.NextIs(TokenType.Operator, "("))
            {
                // It's a constructor
                var ctor = new ConstructorNode
                {
                    Location = accessModifier.Location,
                    AccessModifier = new KeywordNode(accessModifier),
                    Parameters = ParseParameterList(t),
                    Statements = ParseMethodBody(t)
                    // TODO: Class Name?
                };
                if (!(returnType is TypeNode returnTypeNode) || !returnTypeNode.GenericArguments.IsNullOrEmpty())
                    throw new ParsingException($"Invalid name for constructor at {returnType.Location}");
                ctor.ClassName = returnTypeNode.Name;
                node = ctor;
            }
            else
            {
                var name = t.Expect(TokenType.Identifier);
                node = new MethodNode
                {
                    Location = accessModifier.Location,
                    AccessModifier = new KeywordNode(accessModifier),
                    ReturnType = returnType,
                    Name = new IdentifierNode(name),
                    Parameters = ParseParameterList(t),
                    Statements = ParseMethodBody(t)
                };
            }

            return node;
        }

        private static List<AstNode> ParseParameterList(Tokenizer t)
        {
            t.Expect(TokenType.Operator, "(");
            var parameterList = new List<AstNode>();

            t.Expect(TokenType.Operator, ")");
            return parameterList;
        }

        private List<AstNode> ParseMethodBody(Tokenizer t)
        {
            var lookahead = t.Peek();
            if (lookahead.IsOperator("=>"))
                return ParseExpressionBodiedMethodBody(t);

            if (lookahead.IsOperator("{"))
                return ParseNormalMethodBody(t);
            throw ParsingException.CouldNotParseRule(nameof(ParseMethodBody), lookahead);
        }

        private List<AstNode> ParseExpressionBodiedMethodBody(Tokenizer t)
        {
            var lambdaToken = t.Expect(TokenType.Operator, "=>");
            var expr = ParseExpression(t);
            t.Expect(TokenType.Operator, ";");
            return new List<AstNode>
            {
                new ReturnNode
                {
                    Location = lambdaToken.Location,
                    Expression = expr
                }
            };
        }

        private List<AstNode> ParseNormalMethodBody(Tokenizer t)
        {
            t.Expect(TokenType.Operator, "{");
            var statements = new List<AstNode>();
            while (true)
            {
                var stmt = ParseStatement(t);
                if (stmt == null)
                    break;
                statements.Add(stmt);
            }

            t.Expect(TokenType.Operator, "}");
            return statements;
        }
    }
}

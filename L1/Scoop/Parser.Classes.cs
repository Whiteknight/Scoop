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
                    // l2 is either a return type or a class name for a constructor
                    var l2 = t.Expect(TokenType.Identifier);
                    var l3 = t.Peek();
                    if (l3.Is(TokenType.Operator, "("))
                    {
                        // It's a constructor
                        t.PutBack(l2);
                        t.PutBack(lookahead);
                        var ctor = ParseConstructor(t);
                        members.Add(ctor);
                        continue;
                    }
                    // It's a method
                    t.PutBack(l2);
                    t.PutBack(lookahead);
                    var method = ParseMethod(t);
                    members.Add(method);
                    continue;
                }

                throw ParsingException.CouldNotParseRule(nameof(ParseClassBody), lookahead);
            }

            return members;
        }

        private ConstructorNode ParseConstructor(Tokenizer t)
        {
            var accessModifier = t.Expect(TokenType.Keyword, "public", "private");
            var className = t.Expect(TokenType.Identifier);
            var ctorNode = new ConstructorNode
            {
                Location = accessModifier.Location,
                AccessModifier = new KeywordNode(accessModifier),
                ClassName = new IdentifierNode(className)
            };
            t.Expect(TokenType.Operator, "(");
            ctorNode.Parameters = new List<AstNode>();
            t.Expect(TokenType.Operator, ")");
            t.Expect(TokenType.Operator, "{");
            ctorNode.Statements = new List<AstNode>();
            t.Expect(TokenType.Operator, "}");
            return ctorNode;
        }

        // Helper method to start parsing at the method level, mostly to simplify unit tests
        public MethodNode ParseMethod(string s) => ParseMethod(new Tokenizer(new StringCharacterSequence(s)));

        private MethodNode ParseMethod(Tokenizer t)
        {
            var accessModifier = t.Expect(TokenType.Keyword, "public", "private");
            var returnType = t.Expect(TokenType.Identifier);
            var name = t.Expect(TokenType.Identifier);
            var methodNode = new MethodNode
            {
                Location = accessModifier.Location,
                AccessModifier = new KeywordNode(accessModifier),
                ReturnType = new IdentifierNode(returnType),
                Name = new IdentifierNode(name)
            };
            var parameterList = ParseParameterList(t);
            methodNode.Parameters = parameterList;
            methodNode.Statements = ParseMethodBody(t);
            
            return methodNode;
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

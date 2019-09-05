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
            var genericTypeParams = ParseGenericTypeParametersList(t);
            var classNode = new ClassNode
            {
                AccessModifier = new KeywordNode(accessModifierToken),
                Name = new IdentifierNode(classNameToken),
                GenericTypeParameters = genericTypeParams,
            };
            if (t.NextIs(TokenType.Operator, ":", true))
            {
                classNode.Interfaces = new List<AstNode>();
                var contractType = ParseType(t);
                classNode.Interfaces.Add(contractType);
                while (t.NextIs(TokenType.Operator, ",", true))
                {
                    contractType = ParseType(t);
                    classNode.Interfaces.Add(contractType);
                }
            }

            // TODO: "where" <genericTypeParameter> ":" <typeConstraints>

            t.Expect(TokenType.Operator, "{");
            classNode.Members = ParseClassBody(t);
            t.Expect(TokenType.Operator, "}");
            return classNode;
        }

        private List<AstNode> ParseClassBody(Tokenizer t)
        {
            var members = new List<AstNode>();
            while (true)
            {
                var lookaheads = t.Peek(2);
                if (lookaheads[0].Is(TokenType.Operator, "}"))
                    break;

                if (lookaheads[0].IsType(TokenType.CSharpLiteral))
                {
                    t.Advance();
                    members.Add(new CSharpNode(lookaheads[0]));
                    continue;
                }

                if (lookaheads[0].IsKeyword("public", "private"))
                {
                    if (lookaheads[1].IsKeyword("class"))
                    {
                        var nestedClass = ParseClass(t);
                        members.Add(nestedClass);
                        continue;
                    }
                    if (lookaheads[1].IsKeyword("interface"))
                    {
                        var nestedClass = ParseInterface(t);
                        members.Add(nestedClass);
                        continue;
                    }

                    var member = ParseConstructorOrMethod(t);
                    members.Add(member);
                    continue;
                }

                throw ParsingException.CouldNotParseRule(nameof(ParseClassBody), lookaheads[0]);
            }

            return members;
        }

        public AstNode ParseConstructorOrMethod(string s) => ParseConstructorOrMethod(new Tokenizer(new StringCharacterSequence(s)));

        private AstNode ParseConstructorOrMethod(Tokenizer t)
        {
            var accessModifier = t.Expect(TokenType.Keyword, "public", "private");
            // TODO: "async"
            var returnType = ParseType(t);
            AstNode node = null;
            if (t.NextIs(TokenType.Operator, "("))
            {
                // It's a constructor
                // TODO: ":" "this" "(" <args> ")"
                var ctor = new ConstructorNode
                {
                    Location = accessModifier.Location,
                    AccessModifier = new KeywordNode(accessModifier),
                    Parameters = ParseParameterList(t),
                    Statements = ParseMethodBody(t)
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
                    GenericTypeParameters = ParseGenericTypeParametersList(t),
                    Parameters = ParseParameterList(t),
                    // TODO: "where" <genericTypeParameter> ":" <typeConstraints>
                    Statements = ParseMethodBody(t)
                };
            }

            return node;
        }

        private List<AstNode> ParseGenericTypeParametersList(Tokenizer t)
        {
            if (!t.NextIs(TokenType.Operator, "<"))
                return null;
            var types = new List<AstNode>();
            t.Expect(TokenType.Operator, "<");
            var type = ParseType(t);
            types.Add(type);
            while (t.NextIs(TokenType.Operator, ",", true))
            {
                type = ParseType(t);
                types.Add(type);
            }
            t.Expect(TokenType.Operator, ">");
            return types;
        }

        private List<AstNode> ParseParameterList(Tokenizer t)
        {
            t.Expect(TokenType.Operator, "(");
            var parameterList = new List<AstNode>();
            var lookahead = t.Peek();
            if (!lookahead.IsOperator(")"))
            {
                while (true)
                {
                    var type = ParseType(t);
                    var nameToken = t.Expect(TokenType.Identifier);
                    var parameter = new ParameterNode
                    {
                        Location = type.Location,
                        Type = type,
                        Name = new IdentifierNode(nameToken)
                    };
                    if (t.Peek().IsOperator("="))
                    {
                        t.Advance();
                        parameter.DefaultValue = ParseExpression(t);
                    }

                    parameterList.Add(parameter);
                    lookahead = t.Peek();
                    if (lookahead.IsOperator(","))
                    {
                        t.Advance();
                        continue;
                    }

                    if (lookahead.IsOperator(")"))
                        break;

                    throw ParsingException.CouldNotParseRule(nameof(ParseParameterList), lookahead);
                }
            }

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
            if (t.NextIs(TokenType.Operator, "}", true))
                return statements;
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

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
            var classNode = new ClassNode
            {
                AccessModifier = new KeywordNode(t.Expect(TokenType.Keyword, "public", "private")),
            };
            if (t.Peek().IsKeyword("partial"))
                classNode.Modifiers = new List<KeywordNode> { new KeywordNode(t.GetNext()) };
            classNode.Type = new KeywordNode(t.Expect(TokenType.Keyword, "class", "struct"));
            classNode.Name = new IdentifierNode(t.Expect(TokenType.Identifier));
            classNode.GenericTypeParameters = ParseGenericTypeParametersList(t);
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
            classNode.TypeConstraints = ParseTypeConstraints(t);
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
                var lookahead = t.Peek();
                if (lookahead.Is(TokenType.Operator, "}"))
                    break;

                if (lookahead.IsType(TokenType.CSharpLiteral))
                {
                    members.Add(new CSharpNode(t.GetNext()));
                    continue;
                }

                var lookaheads = t.Peek(2);
                // For now, everything must have explicit "public" or "private". We don't infer
                // "private" if neither is specified.
                // TODO: Allow no-modifier to infer "private"
                if (lookaheads[0].IsKeyword("public", "private") && lookaheads[1].IsKeyword("class", "struct"))
                {
                    members.Add(ParseClass(t));
                    continue;
                }

                if (lookaheads[0].IsKeyword("public", "private") && lookaheads[1].IsKeyword("interface"))
                {
                    members.Add(ParseInterface(t));
                    continue;
                }
                // TODO: enum

                var member = ParseClassMember(t);
                members.Add(member);
            }

            return members;
        }

        public AstNode ParseClassMember(string s) => ParseClassMember(new Tokenizer(s));
        private AstNode ParseClassMember(Tokenizer t)
        {
            var accessModifier = t.Peek().IsKeyword("public", "private") ? new KeywordNode(t.GetNext()) : null;
            if (t.NextIs(TokenType.Keyword, "const"))
            {
                // constant
                // <accessModifier>? "const" <type> <ident> "=" <expression>  ";"
                var constNode = new ConstNode
                {
                    Location = t.GetNext().Location,
                    AccessModifier = accessModifier,
                    Type = ParseType(t),
                    Name = new IdentifierNode(t.Expect(TokenType.Identifier))
                };
                t.Expect(TokenType.Operator, "=");
                constNode.Value = ParseExpression(t);
                t.Expect(TokenType.Operator, ";");
                return constNode;
            }

            var asyncModifier = t.NextIs(TokenType.Keyword, "async") ? new KeywordNode(t.GetNext()) : null;
            var returnType = ParseType(t);
            if (asyncModifier == null && t.NextIs(TokenType.Operator, "("))
            {
                // Constructor
                // <accessModifier>? <type> <parameterList> <methodBody>
                // TODO: ":" "this" "(" <args> ")"
                var ctor = new ConstructorNode
                {
                    Location = returnType.Location,
                    AccessModifier = accessModifier,
                    Parameters = ParseParameterList(t),
                    Statements = ParseMethodBody(t)
                };
                if (!(returnType is TypeNode returnTypeNode) || !returnTypeNode.GenericArguments.IsNullOrEmpty())
                    throw new ParsingException($"Invalid name for constructor at {returnType.Location}");
                ctor.ClassName = returnTypeNode.Name;
                return ctor;
            }

            var name = new IdentifierNode(t.Expect(TokenType.Identifier));
            if (asyncModifier != null || t.Peek().IsOperator("<", "("))
            {
                // Method
                // <accessModifier>? "async"? <type> <ident> <genericTypeParameters>? <parameterList> <typeConstraints>? <methodBody>
                var method = new MethodNode
                {
                    Location = name.Location,
                    AccessModifier = accessModifier
                };
                if (asyncModifier != null)
                    method.AddModifier(asyncModifier);
                method.ReturnType = returnType;
                method.Name = name;
                method.GenericTypeParameters = ParseGenericTypeParametersList(t);
                method.Parameters = ParseParameterList(t);
                method.TypeConstraints = ParseTypeConstraints(t);
                method.Statements = ParseMethodBody(t);
                return method;
            }

            if (accessModifier == null)
            {
                // It's a field. Fields may not have access modifier, because they are always private
                // <type> <ident> ";"
                var field = new FieldNode
                {
                    Location = name.Location,
                    Type = returnType,
                    Name = name
                };
                t.Expect(TokenType.Operator, ";");
                return field;
            }

            throw ParsingException.CouldNotParseRule(nameof(ParseClassMember), t.Peek());
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
            // TODO: Method-level const values
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

        private List<TypeConstraintNode> ParseTypeConstraints(Tokenizer t)
        {
            if (!t.Peek().IsKeyword("where"))
                return null;
            var constraints = new List<TypeConstraintNode>();
            while (t.Peek().IsKeyword("where"))
            {
                var constraint = new TypeConstraintNode
                {
                    Location = t.GetNext().Location,
                    Type = new IdentifierNode(t.Expect(TokenType.Identifier)),
                    Constraints = new List<AstNode>()
                };
                t.Expect(TokenType.Operator, ":");
                while (true)
                {
                    var next = t.Peek(3);
                    if (next[0].IsKeyword("new") && next[1].IsOperator("(") && next[2].IsOperator(")"))
                    {
                        t.Advance(3);
                        constraint.Constraints.Add(new KeywordNode
                        {
                            Location = next[0].Location,
                            Keyword = "new()"
                        });
                    }

                    else if (next[0].IsKeyword("class"))
                        constraint.Constraints.Add(new KeywordNode(t.GetNext()));
                    else
                    {
                        var type = ParseType(t);
                        constraint.Constraints.Add(type);
                    }

                    if (t.NextIs(TokenType.Operator, ",", true))
                        continue;
                    break;
                }

                constraints.Add(constraint);
            }

            return constraints;
        }
    }
}

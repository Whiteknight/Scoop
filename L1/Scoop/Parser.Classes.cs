using System.Collections.Generic;
using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop
{
    public partial class Parser
    {
        // Helper method to start parsing at the class level, mostly to simplify unit tests
        public ClassNode ParseClass(string s) => ParseClass(new Tokenizer(s), null);

        private ClassNode ParseClass(Tokenizer t, List<AttributeNode> attributes)
        {
            return new ClassNode
            {
                Attributes = attributes ?? ParseAttributes(t),
                AccessModifier = new KeywordNode(t.Expect(TokenType.Keyword, "public", "private")),
                Modifiers = t.NextIs(TokenType.Keyword, "partial") ? new List<KeywordNode> { new KeywordNode(t.GetNext()) } : null,
                Type = new KeywordNode(t.Expect(TokenType.Keyword, "class", "struct")),
                Name = new IdentifierNode(t.Expect(TokenType.Identifier)),
                GenericTypeParameters = ParseGenericTypeParametersList(t),
                Interfaces = t.NextIs(TokenType.Operator, ":", true) ? ParseInheritanceList(t) : null,
                TypeConstraints = ParseTypeConstraints(t),
                Members = ParseClassBody(t)
            };
        }

        private List<AstNode> ParseClassBody(Tokenizer t)
        {
            t.Expect(TokenType.Operator, "{");
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

                var attributes = ParseAttributes(t);
                var lookaheads = t.Peek(2);
                // For now, everything must have explicit "public" or "private". We don't infer
                // "private" if neither is specified.
                // TODO: Allow no-modifier to infer "private"
                if (lookaheads[0].IsKeyword("public", "private") && lookaheads[1].IsKeyword("class", "struct"))
                {
                    var classMember = ParseClass(t, attributes);
                    members.Add(classMember);
                    continue;
                }

                if (lookaheads[0].IsKeyword("public", "private") && lookaheads[1].IsKeyword("interface"))
                {
                    var ifaceMember = ParseInterface(t, attributes);
                    members.Add(ifaceMember);
                    continue;
                }
                if (lookaheads[0].IsKeyword("public", "private") && lookaheads[1].IsKeyword("enum"))
                {
                    var enumMember = ParseEnum(t, attributes);
                    members.Add(enumMember);
                    continue;
                }
                if (lookaheads[0].IsKeyword("public", "private") && lookaheads[1].IsKeyword("delegate"))
                {
                    var delegateMember = ParseDelegate(t, attributes);
                    members.Add(delegateMember);
                    continue;
                }

                var member = ParseClassMember(t, attributes);
                members.Add(member);
            }
            t.Expect(TokenType.Operator, "}");
            return members;
        }

        public AstNode ParseClassMember(string s) => ParseClassMember(new Tokenizer(s), null);
        private AstNode ParseClassMember(Tokenizer t, List<AttributeNode> attributes)
        {
            attributes = attributes ?? ParseAttributes(t);
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
                var ctor = new ConstructorNode
                {
                    Attributes = attributes,
                    Location = returnType.Location,
                    AccessModifier = accessModifier,
                    Parameters = ParseParameterList(t),
                    ThisArgs = ParseConstructorThisArgs(t),
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
                    Attributes = attributes,
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
                    Attributes = attributes,
                    Location = name.Location,
                    Type = returnType,
                    Name = name
                };
                t.Expect(TokenType.Operator, ";");
                return field;
            }

            throw ParsingException.CouldNotParseRule(nameof(ParseClassMember), t.Peek());
        }

        private List<AstNode> ParseConstructorThisArgs(Tokenizer t)
        {
            if (!t.NextIs(TokenType.Operator, ":", true))
                return null;
            t.Expect(TokenType.Identifier, "this");
            return ParseArgumentList(t);
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

        private List<ParameterNode> ParseParameterList(Tokenizer t)
        {
            t.Expect(TokenType.Operator, "(");
            var parameterList = new List<ParameterNode>();
            if (t.NextIs(TokenType.Operator, ")", true))
                return parameterList;

            while (true)
            {
                var parameter = new ParameterNode
                {
                    Attributes = ParseAttributes(t),
                    Type = ParseType(t),
                    Name = new IdentifierNode(t.Expect(TokenType.Identifier)),
                    DefaultValue = t.NextIs(TokenType.Operator, "=", true) ? ParseExpression(t) : null
                };
                parameter.Location = parameter.Type.Location;
                parameterList.Add(parameter);
                var lookahead = t.Peek();
                if (lookahead.IsOperator(","))
                {
                    t.Advance();
                    continue;
                }

                if (lookahead.IsOperator(")"))
                    break;

                throw ParsingException.CouldNotParseRule(nameof(ParseParameterList), lookahead);
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
    }
}

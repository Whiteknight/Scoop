﻿using System.Collections.Generic;
using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop
{
    public partial class Parser
    {

        private AstNode ParseType(Tokenizer t)
        {
            return ParseTypeArray(t);
        }

        private AstNode ParseTypeArray(Tokenizer t)
        {
            var type = ParseTypeSubtype(t);
            while (true)
            {
                var lookahead = t.Peek();
                if (lookahead.IsOperator("["))
                {
                    t.Advance();
                    // TODO: Size
                    // TODO: "[" a, b, c "]" multi-dimensional array syntax
                    t.Expect(TokenType.Operator, "]");
                    type = new ArrayTypeNode
                    {
                        ElementType = type,
                        Location = lookahead.Location
                    };
                    continue;
                }

                break;
            }

            return type;
        }

        private AstNode ParseTypeSubtype(Tokenizer t)
        {
            // <generic>
            // <generic> "." <generic>
            AstNode type = ParseTypeGeneric(t);
            while (t.NextIs(TokenType.Operator, ".", true))
            {
                var child = ParseTypeGeneric(t);
                type = new ChildTypeNode
                {
                    Location = type.Location,
                    Parent = type,
                    Child = child
                };
            }

            return type;
        }

        private TypeNode ParseTypeGeneric(Tokenizer t)
        {
            // <identifier>
            // <identifier> "<" <typeName> ">"
            var typeNode = ParseTypeName(t);
            if (typeNode == null)
                return null;

            if (t.Peek().IsOperator("<"))
            {
                t.Advance();
                typeNode.GenericArguments = new List<AstNode>();
                while (true)
                {
                    var elementType = ParseTypeArray(t);
                    if (elementType == null)
                        return null;
                    typeNode.GenericArguments.Add(elementType);
                    if (!t.NextIs(TokenType.Operator, ",", true))
                        break;
                }

                t.Expect(TokenType.Operator, ">");
            }

            return typeNode;
        }

        private TypeNode ParseTypeName(Tokenizer t)
        {
            var id = t.Peek();
            if (!id.IsType(TokenType.Identifier))
                return null;
            t.Advance();

            return new TypeNode
            {
                Location = id.Location,
                Name = new IdentifierNode(id)
            };
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

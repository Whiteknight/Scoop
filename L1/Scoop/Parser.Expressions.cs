using System;
using System.Collections.Generic;
using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop
{
    public partial class Parser
    {
        private AstNode ParseExpression(Tokenizer t)
        {
            // Top-level general-purpose expression parsing method, redirects to the appropriate
            // precidence level
            return ParseExpressionAssignment(t);
        }

        private AstNode ParseExpressionAssignment(Tokenizer t)
        {
            // Operators with Assignment precidence
            // <Conditional> (<op> <Conditional>)+
            var left = ParseExpressionConditional(t);
            while (t.Peek().IsOperator("=", "+=", "-=", "/=", "%="))
            {
                var op = new OperatorNode(t.GetNext());
                var right = ParseExpressionConditional(t);
                left = new InfixOperationNode
                {
                    Location = op.Location,
                    Left = left,
                    Operator = op,
                    Right = right
                };
            }

            return left;
        }

        private AstNode ParseExpressionConditional(Tokenizer t)
        {
            // TODO: Conditional (ternary)
            return ParseExpressionLogical(t);
        }

        private AstNode ParseExpressionLogical(Tokenizer t)
        {
            // Operators with + - precidence
            // <Bitwise> (<op> <Bitwise>)+
            var left = ParseExpressionBitwise(t);
            while (t.Peek().IsOperator("&&", "||"))
            {
                var op = new OperatorNode(t.GetNext());
                var right = ParseExpressionBitwise(t);
                left = new InfixOperationNode
                {
                    Location = op.Location,
                    Left = left,
                    Operator = op,
                    Right = right
                };
            }

            return left;
        }

        private AstNode ParseExpressionBitwise(Tokenizer t)
        {
            // Operators with + - precidence
            // <Equality> (<op> <Equality>)+
            var left = ParseExpressionEquality(t);
            while (t.Peek().IsOperator("&", "^", "|"))
            {
                var op = new OperatorNode(t.GetNext());
                var right = ParseExpressionEquality(t);
                left = new InfixOperationNode
                {
                    Location = op.Location,
                    Left = left,
                    Operator = op,
                    Right = right
                };
            }

            return left;
        }

        private AstNode ParseExpressionEquality(Tokenizer t)
        {
            // Operators with + - precidence
            // <Additive> (<op> <Additive>)+
            var left = ParseExpressionAdditive(t);
            while (t.Peek().IsOperator("==", "!=", ">=", "<=", "<", ">"))
            {
                var op = new OperatorNode(t.GetNext());
                var right = ParseExpressionAdditive(t);
                left = new InfixOperationNode
                {
                    Location = op.Location,
                    Left = left,
                    Operator = op,
                    Right = right
                };
            }

            return left;
        }

        // TODO: "is" and "as" go here, probably

        private AstNode ParseExpressionAdditive(Tokenizer t)
        {
            // Operators with + - precidence
            // <Multiplicative> (<op> <Multiplicative>)+
            var left = ParseExpressionMultiplicative(t);
            while (t.Peek().IsOperator("+", "-"))
            {
                var op = new OperatorNode(t.GetNext());
                var right = ParseExpressionMultiplicative(t);
                left = new InfixOperationNode
                {
                    Location = op.Location,
                    Left = left,
                    Operator = op,
                    Right = right
                };
            }

            return left;
        }

        private AstNode ParseExpressionMultiplicative(Tokenizer t)
        {
            // Operators with * / % precidence
            // <Unary> (<op> <Unary>)+
            var left = ParseExpressionUnary(t);
            while (t.Peek().IsOperator("*", "/", "%"))
            {
                var op = new OperatorNode(t.GetNext());
                var right = ParseExpressionUnary(t);
                left = new InfixOperationNode
                {
                    Location = op.Location,
                    Left = left,
                    Operator = op,
                    Right = right
                };
            }

            return left;
        }

        private AstNode ParseExpressionUnary(Tokenizer t)
        {
            // ("-" | "+" | "~") <Postfix> | <Postfix>
            var next = t.Peek();
            // TODO: Loop to get all possible prefixes
            // TODO: <cast> <expr>
            if (next.IsOperator("-", "+", "++", "--", "!", "~"))
            {
                var op = t.GetNext();
                var expr = ParseExpressionPostfix(t);
                return new PrefixOperationNode
                {
                    Location = op.Location,
                    Operator = new OperatorNode(op),
                    Right = expr
                };
            }

            return ParseExpressionPostfix(t);
        }

        private AstNode ParseExpressionPostfix(Tokenizer t)
        {
            var current = ParseExpressionTerminal(t);
            while (true)
            {
                var lookahead = t.Peek();

                // <terminal> ("++" | "--")
                if (lookahead.IsOperator("++", "--"))
                {
                    t.Advance();
                    current = new PostfixOperationNode
                    {
                        Location = current.Location,
                        Left = current,
                        Operator = new OperatorNode(lookahead)
                    };
                    continue;
                }

                if (lookahead.IsOperator("."))
                {
                    t.Advance();
                    var identifier = t.Expect(TokenType.Identifier);
                    current = new MemberAccessNode
                    {
                        Location = lookahead.Location,
                        Instance = current,
                        MemberName = new IdentifierNode(identifier)
                    };
                    continue;
                }

                if (lookahead.IsOperator("("))
                {
                    t.Advance();
                    // TODO: Args
                    current = new InvokeNode
                    {
                        Arguments = new List<AstNode>(),
                        Instance = current,
                        Location = lookahead.Location
                    };
                    t.Expect(TokenType.Operator, ")");
                    continue;
                }

                // TODO: <terminal> "[" <args> "]"
                return current;
            }
        }

        private AstNode ParseExpressionTerminal(Tokenizer t)
        {
            // Terminal expression
            // "true" | "false" | "null" | <Identifier> | <String> | <Number> | "new" <type> "(" <args> ")" | "(" <Expression> ")"
            var lookahead = t.Peek();

            if (lookahead.IsKeyword("true", "false", "null"))
                return new KeywordNode(t.GetNext());

            if (lookahead.IsType(TokenType.Identifier))
                return new IdentifierNode(t.GetNext());

            if (lookahead.IsType(TokenType.String))
                return new StringNode(t.GetNext());
            if (lookahead.IsType(TokenType.Character))
                return new CharNode(t.GetNext());
            if (lookahead.IsType(TokenType.Integer))
                return new IntegerNode(t.GetNext());
            if (lookahead.IsType(TokenType.UInteger))
                return new UIntegerNode(t.GetNext());
            if (lookahead.IsType(TokenType.Long))
                return new LongNode(t.GetNext());
            if (lookahead.IsType(TokenType.ULong))
                return new ULongNode(t.GetNext());
            if (lookahead.IsType(TokenType.Decimal))
                return new DecimalNode(t.GetNext());
            if (lookahead.IsType(TokenType.Float))
                return new FloatNode(t.GetNext());
            if (lookahead.IsType(TokenType.Double))
                return new DoubleNode(t.GetNext());

            if (lookahead.IsKeyword("new"))
            {
                var newToken = t.GetNext();
                var typeNode = ParseType(t);
                t.Expect(TokenType.Operator, "(");
                // TODO: Argument list
                t.Expect(TokenType.Operator, ")");
                return new NewNode
                {
                    Location = newToken.Location,
                    Type = typeNode,
                    Arguments = new List<AstNode>()
                };
            }

            if (lookahead.IsOperator("("))
            {
                var value = ParseParenthesis(t, x => ParseExpression(t));
                return value.Expression;
            }

            throw ParsingException.CouldNotParseRule(nameof(ParseExpressionTerminal), lookahead);
        }

        private ParenthesisNode<TNode> ParseParenthesis<TNode>(Tokenizer t, Func<Tokenizer, TNode> parse)
            where TNode : AstNode
        {
            var openingParen = t.Expect(TokenType.Operator, "(");
            if (t.Peek().IsOperator(")"))
            {
                t.GetNext();
                return new ParenthesisNode<TNode>
                {
                    Location = openingParen.Location
                };
            }

            var value = parse(t);
            t.Expect(TokenType.Operator, ")");
            return new ParenthesisNode<TNode>
            {
                Location = openingParen.Location,
                Expression = value
            };
        }
    }
}

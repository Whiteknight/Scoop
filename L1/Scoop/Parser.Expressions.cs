using System;
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
            // <Expression2> (<op> <Expression2>)+
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
            // <Expression2> (<op> <Expression2>)+
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
            // <Expression2> (<op> <Expression2>)+
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
            // <Expression2> (<op> <Expression2>)+
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

        private AstNode ParseExpressionAdditive(Tokenizer t)
        {
            // Operators with + - precidence
            // <Expression2> (<op> <Expression2>)+
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
            // <Expression1> (<op> <Expression1>)+
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
            // ("-" | "+" | "~") <Expression0> | <Expression0>
            var next = t.Peek();
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
            var terminal = ParseExpressionTerminal(t);
            // TODO: <terminal> ("++" | "--")
            // TODO: <terminal> "(" <args> ")"
            // TODO: <terminal> "." <identifer> ( "(" <args> ")" )?
            return terminal;
        }

        // Parse terminals
        private AstNode ParseExpressionTerminal(Tokenizer t)
        {
            // Terminal expression
            // <Identifier> | <Variable> | <String> | <Number> | "(" <Expression> ")"
            var lookahead = t.Peek();

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


            if (lookahead.IsOperator("("))
            {
                // "(" (<QueryExpression> | <ScalarExpression>) ")"
                // e.g. SET @x = (select 5) or INSERT INTO x(num) VALUES (1, 2, (select 3))
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

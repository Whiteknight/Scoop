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
            return ParseExpression4(t);
        }

        private AstNode ParseExpression4(Tokenizer t)
        {
            // TODO: Ternary Operator

            return ParseExpression3(t);
        }

        private AstNode ParseExpression3(Tokenizer t)
        {
            // Operators with + - precidence
            // <Expression2> (<op> <Expression2>)+
            var left = ParseExpression2(t);
            while (t.Peek().IsOperator("+", "-", "&", "^", "|"))
            {
                var op = new OperatorNode(t.GetNext());
                var right = ParseExpression2(t);
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

        private AstNode ParseExpression2(Tokenizer t)
        {
            // Operators with * / % precidence
            // <Expression1> (<op> <Expression1>)+
            var left = ParseExpression1(t);
            while (t.Peek().IsOperator("*", "/", "%"))
            {
                var op = new OperatorNode(t.GetNext());
                var right = ParseExpression1(t);
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

        private AstNode ParseExpression1(Tokenizer t)
        {
            // ("-" | "+" | "~") <Expression0> | <Expression0>
            var next = t.Peek();
            if (next.IsOperator("-", "+"))
            {
                var op = t.GetNext();
                var expr = ParseExpression1(t);
                return new PrefixOperationNode
                {
                    Location = op.Location,
                    Operator = new OperatorNode(op),
                    Right = expr
                };
            }

            return ParseExpression0(t);
        }

        private AstNode ParseExpression0(Tokenizer t)
        {
            // Terminal expression
            // <MethodCall> | <Identifier> | <Variable> | <String> | <Number> | "(" <Expression> ")"
            var next = t.Peek();
            

            if (next.IsType(TokenType.String))
                return new StringNode(t.GetNext());
            if (next.IsType(TokenType.Character))
                return new CharNode(t.GetNext());
            if (next.IsType(TokenType.Integer))
                return new IntegerNode(t.GetNext());
            if (next.IsType(TokenType.UInteger))
                return new UIntegerNode(t.GetNext());
            if (next.IsType(TokenType.Long))
                return new LongNode(t.GetNext());
            if (next.IsType(TokenType.ULong))
                return new ULongNode(t.GetNext());
            if (next.IsType(TokenType.Decimal))
                return new DecimalNode(t.GetNext());
            if (next.IsType(TokenType.Float))
                return new FloatNode(t.GetNext());
            if (next.IsType(TokenType.Double))
                return new DoubleNode(t.GetNext());


            if (next.IsOperator("("))
            {
                // "(" (<QueryExpression> | <ScalarExpression>) ")"
                // e.g. SET @x = (select 5) or INSERT INTO x(num) VALUES (1, 2, (select 3))
                var value = ParseParenthesis(t, x => ParseExpression(t));
                return value.Expression;
            }

            throw ParsingException.CouldNotParseRule(nameof(ParseExpression0), next);
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

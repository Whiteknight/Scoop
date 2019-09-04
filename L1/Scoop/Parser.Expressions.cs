using System;
using System.Collections.Generic;
using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop
{
    // TODO: "await"
    public partial class Parser
    {
        public AstNode ParseExpression(string s) => ParseExpression(new Tokenizer(s));
        private AstNode ParseExpression(Tokenizer t)
        {
            // Top-level general-purpose expression parsing method, redirects to the appropriate
            // precidence level
            return ParseExpressionComma(t);
        }

        private AstNode ParseExpressionNonComma(Tokenizer t)
        {
            // Top-level expression parsing method for situations where the comma operator is not
            // allowed.
            return ParseExpressionLambda(t);
        }

        private AstNode ParseExpressionComma(Tokenizer t)
        {
            var left = ParseExpressionLambda(t);
            if (t.Peek().IsOperator(","))
            {
                var items = new List<AstNode>();
                items.Add(left);
                while(t.Peek().IsOperator(","))
                {
                    t.Advance();
                    var next = ParseExpressionLambda(t);
                    items.Add(next);
                }

                return new ListNode
                {
                    Location = left.Location,
                    Items = items
                };
            }

            return left;
        }

        private AstNode ParseExpressionLambda(Tokenizer t)
        {
            // "() =>" case, which the expression parser won't deal with
            var l1 = t.GetNext();
            var l2 = t.GetNext();
            var l3 = t.GetNext();
            if (l1.IsOperator("(") && l2.IsOperator(")") && l3.IsOperator("=>"))
            {
                var lambdaToken = l3;
                var lambdaNode = new LambdaNode
                {
                    Location = lambdaToken.Location,
                    Parameters = new List<AstNode>()
                };

                if (t.Peek().IsOperator("{"))
                    lambdaNode.Statements = ParseNormalMethodBody(t);
                else
                {
                    var bodyExpr = ParseExpressionAssignment(t);
                    lambdaNode.Statements = new List<AstNode> { bodyExpr };
                }

                return lambdaNode;
            }
            t.PutBack(l3);
            t.PutBack(l2);
            t.PutBack(l1);

            // Otherwise parse the expression and see if it looks like the start of a lambda
            var expr = ParseExpressionAssignment(t);
            if ((expr is IdentifierNode || expr is ListNode) && t.Peek().IsOperator("=>"))
            {
                var lambdaToken = t.GetNext();
                var lambdaNode = new LambdaNode
                {
                    Location = lambdaToken.Location,
                };
                if (expr is IdentifierNode)
                    lambdaNode.Parameters = new List<AstNode> { expr };
                if (expr is ListNode parameterList)
                    lambdaNode.Parameters = parameterList.Items;

                if (t.Peek().IsOperator("{"))
                    lambdaNode.Statements = ParseNormalMethodBody(t);
                else
                {
                    var bodyExpr = ParseExpressionAssignment(t);
                    lambdaNode.Statements = new List<AstNode> { bodyExpr };
                }

                return lambdaNode;
            }

            return expr;
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
            var left = ParseExpressionCoalesce(t);
            while (t.Peek().IsOperator("?"))
            {
                var op = t.GetNext();
                var ifTrue = ParseExpressionCoalesce(t);
                t.Expect(TokenType.Operator, ":");
                var ifFalse = ParseExpressionCoalesce(t);
                left = new ConditionalNode
                {
                    Location = op.Location,
                    Condition = left,
                    IfTrue = ifTrue,
                    IfFalse = ifFalse
                };
            }

            return left;
        }

        private AstNode ParseExpressionCoalesce(Tokenizer t)
        {
            var left = ParseExpressionLogical(t);
            while (t.Peek().IsOperator("??"))
            {
                var op = new OperatorNode(t.GetNext());
                var right = ParseExpressionLogical(t);
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
            var left = ParseExpressionTypeCoerce(t);
            while (t.Peek().IsOperator("==", "!=", ">=", "<=", "<", ">"))
            {
                var op = new OperatorNode(t.GetNext());
                var right = ParseExpressionTypeCoerce(t);
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

        private AstNode ParseExpressionTypeCoerce(Tokenizer t)
        {
            var left = ParseExpressionAdditive(t);
            var lookahead = t.Peek();
            if (lookahead.IsOperator("is", "as"))
            {
                var op = t.GetNext();
                var type = ParseType(t);
                return new InfixOperationNode
                {
                    Location = op.Location,
                    Left = left,
                    Operator = new OperatorNode(op),
                    Right = type
                };
            }

            return left;
        }

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
            // Probably won't do cast operator because of ambiguous parsing rules
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

        private bool IsGenericTypeArgument(Tokenizer t)
        {
            var putbacks = new Stack<Token>();
            int angleBracketDepth = 0;
            bool success = true;
            while (true)
            {
                var next = t.GetNext();
                putbacks.Push(next);
                if (next.IsOperator("<"))
                {
                    angleBracketDepth++;
                    continue;
                }

                if (next.IsOperator(">"))
                {
                    angleBracketDepth--;
                    if (angleBracketDepth < 0)
                    {
                        success = false;
                        break;
                    }
                    if (angleBracketDepth == 0)
                        break;
                    continue;
                }

                if (next.IsType(TokenType.Identifier) || next.IsOperator(".", "[", "]"))
                    continue;
                success = false;
                break;
            }

            while (putbacks.Count > 0)
                t.PutBack(putbacks.Pop());
            return success && angleBracketDepth == 0;
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

                // member access (property and method)
                // <terminal> "." <identifier>
                // <terminal> "?." <identifier>
                if (lookahead.IsOperator(".", "?."))
                {
                    t.Advance();
                    var identifier = t.Expect(TokenType.Identifier);
                    var memberAccessNode = new MemberAccessNode
                    {
                        Location = lookahead.Location,
                        Instance = current,
                        IgnoreNulls = lookahead.Value == "?.",
                        MemberName = new IdentifierNode(identifier)
                    };
                    if (t.NextIs(TokenType.Operator, "<") && IsGenericTypeArgument(t))
                        memberAccessNode.GenericArguments = ParseGenericTypeParametersList(t);

                    current = memberAccessNode;
                    continue;
                }

                // Invoke operator
                // <terminal> "(" <args> ")"
                if (lookahead.IsOperator("("))
                {
                    var args = ParseArgumentList(t);
                    current = new InvokeNode
                    {
                        Arguments = args,
                        Instance = current,
                        Location = lookahead.Location,
                    };
                    
                    continue;
                }

                if (lookahead.IsOperator("["))
                {
                    var args = ParseIndexArgumentList(t);
                    current = new IndexNode
                    {
                        Location = lookahead.Location,
                        Arguments = args,
                        Instance = current
                    };
                }

                return current;
            }
        }

        private List<AstNode> ParseArgumentList(Tokenizer t)
        {
            if (!t.NextIs(TokenType.Operator, "("))
                return new List<AstNode>();
            t.Expect(TokenType.Operator, "(");
            var args = new List<AstNode>();
            while (true)
            {
                var lookahead = t.Peek();
                if (lookahead.IsOperator(")"))
                    break;
                var arg = ParseExpressionLambda(t);
                args.Add(arg);
                if (t.Peek().IsOperator(","))
                {
                    t.Advance();
                    continue;
                }

                break;
            }

            t.Expect(TokenType.Operator, ")");
            return args;
        }

        private List<AstNode> ParseIndexArgumentList(Tokenizer t)
        {
            t.Expect(TokenType.Operator, "[");
            var args = new List<AstNode>();
            while (true)
            {
                var lookahead = t.Peek();
                if (lookahead.IsOperator("]"))
                    break;
                var arg = ParseExpressionLambda(t);
                args.Add(arg);
                if (t.Peek().IsOperator(","))
                {
                    t.Advance();
                    continue;
                }

                break;
            }

            t.Expect(TokenType.Operator, "]");
            return args;
        }

        private AstNode ParseExpressionTerminal(Tokenizer t)
        {
            // Terminal expression
            // Some of these "terminal" values may themselves be productions, but
            // are treated as a single value for the purposes of the expression parser
            // "true" | "false" | "null" | <Identifier> | <String> | <Number> | <new> | "(" <Expression> ")"
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
                return ParseNew(t);

            if (lookahead.IsOperator("("))
            {
                t.Expect(TokenType.Operator, "(");
                if (t.Peek().IsOperator(")"))
                    throw ParsingException.CouldNotParseRule(nameof(ParseExpressionTerminal), t.Peek());

                var value = ParseExpression(t);
                t.Expect(TokenType.Operator, ")");
                return value;
            }

            throw ParsingException.CouldNotParseRule(nameof(ParseExpressionTerminal), lookahead);
        }

        private AstNode ParseNew(Tokenizer t)
        {
            var newToken = t.GetNext();
            // TODO: new { } anonymous object syntax
            var typeNode = ParseType(t);
            var args = ParseArgumentList(t);
            var inits = ParseInitializers(t);
            return new NewNode
            {
                Location = newToken.Location,
                Type = typeNode,
                Arguments = args,
                Initializers = inits
            };
        }

        private List<AstNode> ParseInitializers(Tokenizer t)
        {
            if (!t.NextIs(TokenType.Operator, "{", true))
                return null;
            var inits = new List<AstNode>();
            while (true)
            {
                var lookahead = t.Peek();
                if (lookahead.IsOperator("{"))
                {
                    var dictInit = ParseKeyValueInitializer(t);
                    inits.Add(dictInit);
                }
                else if (lookahead.IsOperator("["))
                {
                    var arrayInit = ParseArrayInitializer(t);
                    inits.Add(arrayInit);
                }
                else
                {
                    t.Advance();
                    var l2 = t.Peek();
                    t.PutBack(lookahead);
                    if (lookahead.IsType(TokenType.Identifier) && l2.IsOperator("="))
                    {
                        var init = ParsePropertyInitializer(t);
                        inits.Add(init);
                    }
                    else
                    {
                        var init = ParseExpressionNonComma(t);
                        inits.Add(init);
                    }
                }

                if (t.NextIs(TokenType.Operator, ",", true))
                    continue;
                break;
            }

            t.Expect(TokenType.Operator, "}");
            return inits;
        }

        private AstNode ParseKeyValueInitializer(Tokenizer t)
        {
            var startToken = t.Expect(TokenType.Operator, "{");
            var key = ParseExpressionNonComma(t);
            t.Expect(TokenType.Operator, ",");
            var value = ParseExpressionNonComma(t);
            t.Expect(TokenType.Operator, "}");
            return new KeyValueInitializerNode
            {
                Location = startToken.Location,
                Key = key,
                Value = value
            };
        }

        private AstNode ParseArrayInitializer(Tokenizer t)
        {
            var startToken = t.Expect(TokenType.Operator, "[");
            // TODO: multi-dimentional arrays "[" 0, 1 "]" "=" ...
            var intToken = t.Expect(TokenType.Integer);
            t.Expect(TokenType.Operator, "]");
            t.Expect(TokenType.Operator, "=");
            var value = ParseExpressionNonComma(t);
            return new ArrayInitializerNode
            {
                Location = startToken.Location,
                Key = new IntegerNode(intToken),
                Value = value
            };
        }

        private AstNode ParsePropertyInitializer(Tokenizer t)
        {
            var propertyToken = t.Expect(TokenType.Identifier);
            t.Expect(TokenType.Operator, "=");
            var value = ParseExpressionNonComma(t);
            return new PropertyInitializerNode
            {
                Location = propertyToken.Location,
                Property = new IdentifierNode(propertyToken),
                Value = value
            };
        }
    }
}

using System.Collections.Generic;
using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop
{
    public partial class Parser
    {
        private AstNode ParseExpressionList(ITokenizer t)
        {
            // Top-level general-purpose expression parsing method, allowing the comma operator
            // This is a relatively rare case.
            return ParseExpressionComma(t);
        }

        public AstNode ParseExpression(string s) => ParseExpression(new Tokenizer(s));
        private AstNode ParseExpression(ITokenizer t)
        {
            // Top-level expression parsing method for situations where the comma operator is not
            // allowed. This is the most common case
            return ParseExpressionLambda(t);
        }

        private AstNode ParseExpressionComma(ITokenizer t)
        {
            var left = ParseExpressionLambda(t);
            if (t.Peek().IsOperator(","))
            {
                var items = new List<AstNode>
                {
                    left
                };
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

        private AstNode ParseExpressionLambda(ITokenizer t)
        {
            // "(" ")" "=>" ( <expression> | "{" <methodBody> "}" )
            var lookaheads = t.Peek(3);
            if (lookaheads[0].IsOperator("(") && lookaheads[1].IsOperator(")") && lookaheads[2].IsOperator("=>"))
            {
                var startToken = t.Expect(TokenType.Operator, "(");
                t.Expect(TokenType.Operator, ")");
                t.Expect(TokenType.Operator, "=>");
                var lambdaNode = new LambdaNode
                {
                    Location = startToken.Location,
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

            // <assignmentExpression>
            // <Identifier> "=>" ( <expression> | "{" <methodBody> "}" )
            // "(" <identifierList> ")"  "=>" ( <expression> | "{" <methodBody> "}" )
            var expr = ParseExpressionAssignment(t);
            if ((expr is IdentifierNode || expr is ListNode) && t.Peek().IsOperator("=>"))
            {
                // TODO: if expr is a ListNode, test that all elements are IdentifierNodes
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

        private AstNode ParseExpressionAssignment(ITokenizer t)
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

        private AstNode ParseExpressionConditional(ITokenizer t)
        {
            // <coalesceExpression>
            // <coalesceExpression> "?" <coalesceExpression> ":" <coalesceExpression>
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

        private AstNode ParseExpressionCoalesce(ITokenizer t)
        {
            // <logicalExpression>
            // <logicalExpression> "??" <logicalExpression>
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

        private AstNode ParseExpressionLogical(ITokenizer t)
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

        private AstNode ParseExpressionBitwise(ITokenizer t)
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

        private AstNode ParseExpressionEquality(ITokenizer t)
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

        private AstNode ParseExpressionTypeCoerce(ITokenizer t)
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

        private AstNode ParseExpressionAdditive(ITokenizer t)
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

        private AstNode ParseExpressionMultiplicative(ITokenizer t)
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

        private AstNode ParseExpressionUnary(ITokenizer t)
        {
            // ("-" | "+" | "~") <Postfix> | <Postfix>
            var next = t.Peek();
            // TODO: Loop to get all possible prefixes
            // TODO: <cast> <expr>
            // Probably won't do cast operator because of ambiguous parsing rules
            if (next.IsOperator("-", "+", "++", "--", "!", "~") || next.IsKeyword("await", "throw"))
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

        private bool IsGenericTypeArgument(ITokenizer t)
        {
            // TODO: Needs improvement to be more robust and correct
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

        private AstNode ParseExpressionPostfix(ITokenizer t)
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
                        // TODO: Instead of a bool here, use an OperatorNode
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

        private List<AstNode> ParseArgumentList(ITokenizer t)
        {
            t.Expect(TokenType.Operator, "(");
            var args = new List<AstNode>();
            while (true)
            {
                var lookaheads = t.Peek(2);
                if (lookaheads[0].IsOperator(")"))
                    break;
                if (lookaheads[0].IsType(TokenType.Identifier) && lookaheads[1].IsOperator(":"))
                {
                    var arg = new NamedArgumentNode
                    {
                        Name = new IdentifierNode(t.Expect(TokenType.Identifier)),
                        Separator = new OperatorNode(t.Expect(TokenType.Operator, ":")),
                        Value = ParseExpression(t)
                    };
                    arg.Location = arg.Name.Location;
                    args.Add(arg);
                }
                else
                {
                    var arg = ParseExpressionLambda(t);
                    args.Add(arg);
                }

                if (t.NextIs(TokenType.Operator, ",", true))
                    continue;

                break;
            }

            t.Expect(TokenType.Operator, ")");
            return args;
        }

        private List<AstNode> ParseIndexArgumentList(ITokenizer t)
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

        private AstNode ParseExpressionTerminal(ITokenizer t)
        {
            // Terminal expression
            // Some of these "terminal" values may themselves be productions, but
            // are treated as a single value for the purposes of the expression parser
            // "true" | "false" | "null" | <Identifier> | <String> | <Number> | <new> | "(" <Expression> ")"
            var lookahead = t.GetNext();

            if (lookahead.IsKeyword("true", "false", "null"))
                return new KeywordNode(lookahead);

            if (lookahead.IsType(TokenType.Identifier))
                return new IdentifierNode(lookahead);

            if (lookahead.IsType(TokenType.String))
                return new StringNode(lookahead);
            if (lookahead.IsType(TokenType.Character))
                return new CharNode(lookahead);
            if (lookahead.IsType(TokenType.Integer))
                return new IntegerNode(lookahead);
            if (lookahead.IsType(TokenType.UInteger))
                return new UIntegerNode(lookahead);
            if (lookahead.IsType(TokenType.Long))
                return new LongNode(lookahead);
            if (lookahead.IsType(TokenType.ULong))
                return new ULongNode(lookahead);
            if (lookahead.IsType(TokenType.Decimal))
                return new DecimalNode(lookahead);
            if (lookahead.IsType(TokenType.Float))
                return new FloatNode(lookahead);
            if (lookahead.IsType(TokenType.Double))
                return new DoubleNode(lookahead);

            if (lookahead.IsKeyword("new"))
            {
                t.PutBack(lookahead);
                return ParseNew(t);
            }

            if (lookahead.IsOperator("("))
            {
                if (t.Peek().IsOperator(")"))
                    throw ParsingException.CouldNotParseRule(nameof(ParseExpressionTerminal), t.Peek());

                // Parse expression list, it may be a tuple literal
                var value = ParseExpressionList(t);
                t.Expect(TokenType.Operator, ")");
                return value;
            }

            throw ParsingException.CouldNotParseRule(nameof(ParseExpressionTerminal), lookahead);
        }

        private AstNode ParseNew(ITokenizer t)
        {
            var newToken = t.GetNext();
            var newNode = new NewNode
            {
                Location = newToken.Location
            };
            if (!t.NextIs(TokenType.Operator, "{"))
            {
                newNode.Type = ParseType(t);
                newNode.Arguments = t.NextIs(TokenType.Operator, "(") ? ParseArgumentList(t) : new List<AstNode>();
            }
            newNode.Initializers = ParseInitializers(t);
            return newNode;
        }

        private List<AstNode> ParseInitializers(ITokenizer t)
        {
            if (!t.NextIs(TokenType.Operator, "{", true))
                return null;
            var inits = new List<AstNode>();
            while (true)
            {
                var lookaheads = t.Peek(2);
                if (lookaheads[0].IsOperator("{"))
                {
                    var dictInit = ParseKeyValueInitializer(t);
                    inits.Add(dictInit);
                }
                else if (lookaheads[0].IsOperator("["))
                {
                    var arrayInit = ParseArrayInitializer(t);
                    inits.Add(arrayInit);
                }
                else
                {
                    if (lookaheads[0].IsType(TokenType.Identifier) && lookaheads[1].IsOperator("="))
                    {
                        var init = ParsePropertyInitializer(t);
                        inits.Add(init);
                    }
                    else
                    {
                        var init = ParseExpression(t);
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

        private AstNode ParseKeyValueInitializer(ITokenizer t)
        {
            var startToken = t.Expect(TokenType.Operator, "{");
            var key = ParseExpression(t);
            t.Expect(TokenType.Operator, ",");
            var value = ParseExpression(t);
            t.Expect(TokenType.Operator, "}");
            return new KeyValueInitializerNode
            {
                Location = startToken.Location,
                Key = key,
                Value = value
            };
        }

        private AstNode ParseArrayInitializer(ITokenizer t)
        {
            var startToken = t.Expect(TokenType.Operator, "[");
            // TODO: multi-dimentional arrays "[" 0, 1 "]" "=" ...
            var intToken = t.Expect(TokenType.Integer);
            t.Expect(TokenType.Operator, "]");
            t.Expect(TokenType.Operator, "=");
            var value = ParseExpression(t);
            return new ArrayInitializerNode
            {
                Location = startToken.Location,
                Key = new IntegerNode(intToken),
                Value = value
            };
        }

        private AstNode ParsePropertyInitializer(ITokenizer t)
        {
            var propertyToken = t.Expect(TokenType.Identifier);
            t.Expect(TokenType.Operator, "=");
            var value = ParseExpression(t);
            return new PropertyInitializerNode
            {
                Location = propertyToken.Location,
                Property = new IdentifierNode(propertyToken),
                Value = value
            };
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using Scoop.Parsers;
using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop
{
    public partial class Parser
    {
        private void InitializeExpressions()
        {
            var argumentParser = ScoopParsers.First(
                ScoopParsers.Sequence(
                    new IdentifierParser(),
                    new OperatorParser(":"),
                    ScoopParsers.Deferred(() => Expressions),
                    (name, s, expr) => new NamedArgumentNode { Name = name, Separator = s, Value = expr }
                ),
                ScoopParsers.Deferred(() => Expressions)
            );
            ArgumentLists = ScoopParsers.First(
                ScoopParsers.Sequence(
                    new OperatorParser("("),
                    new OperatorParser(")"),
                    (a, b) => ListNode<AstNode>.Default()
                ),
                ScoopParsers.Sequence(
                    new OperatorParser("("),
                    ScoopParsers.SeparatedList(
                        argumentParser,
                        new OperatorParser(","),
                        items => new ListNode<AstNode> { Items = items.ToList(), Separator = new OperatorNode(",") }
                    ),
                    new OperatorParser(")"),
                    (a, items, c) => items
                )
            );
            // "(" ")" "=>" ( <expression> | "{" <methodBody> "}" )
            // <assignmentExpression>
            // <Identifier> "=>" ( <expression> | "{" <methodBody> "}" )
            // "(" <identifierList> ")"  "=>" ( <expression> | "{" <methodBody> "}" )
            var lambdaParser = ScoopParsers.First(
                ScoopParsers.Sequence(
                    ScoopParsers.First(
                        ScoopParsers.Transform(
                            new IdentifierParser(),
                            id => new ListNode<IdentifierNode> { Separator = new OperatorNode(","), [0] = id }
                        ),
                        ScoopParsers.Sequence(
                            new OperatorParser("("),
                            new OperatorParser(")"),
                            (a, b) => new ListNode<IdentifierNode> { Separator = new OperatorNode(","), Items = new List<IdentifierNode>() }
                        ),
                        ScoopParsers.Sequence(
                            new OperatorParser("("),
                            ScoopParsers.SeparatedList(
                                new IdentifierParser(),
                                new OperatorParser(","),
                                args => new ListNode<IdentifierNode> { Items = args.ToList(), Separator =  new OperatorNode(",") }
                            ),
                            new OperatorParser(")"),
                            (a, items, c) => new ListNode<IdentifierNode> { Separator = new OperatorNode(","), Items = items.ToList() }
                        )
                    ),
                    new OperatorParser("=>"),
                    ScoopParsers.First(
                        ScoopParsers.Deferred(() => NormalMethodBody),
                        ScoopParsers.Transform(
                            new OldStyleRuleParser<AstNode>(ParseExpressionAssignment),
                            body => new ListNode<AstNode> { [0] = body }
                        )
                    ),
                    (parameters, x, body) => new LambdaNode
                    {
                        Parameters = parameters,
                        Location = x.Location,
                        Statements = body
                    }
                ),
                new OldStyleRuleParser<AstNode>(ParseExpressionAssignment)
            );
            Expressions = lambdaParser;

            ExpressionList = ScoopParsers.SeparatedList(
                Expressions,
                new OperatorParser(","),
                items => new ListNode<AstNode> { Items = items.ToList(), Separator = new OperatorNode(",") }
            );
        }

        public AstNode ParseExpression(string s) => Expressions.Parse(new Tokenizer(s)).GetResult();

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
                var type = Types.Parse(t).GetResult();
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
            var indexingParser = ScoopParsers.Sequence(
                new OperatorParser("["),
                ScoopParsers.SeparatedList(
                    ScoopParsers.Deferred(() => Expressions),
                    new OperatorParser(","),
                    items => new ListNode<AstNode> { Items = items.ToList(), Separator = new OperatorNode(",") }
                ),
                new OperatorParser("]"),
                (a, items, b) => items
            );
            var newParser = ScoopParsers.First(
                // "new" "{" <initializers> "}"
                ScoopParsers.Sequence(
                    new KeywordParser("new"),
                    new OldStyleRuleParser<ListNode<AstNode>>(ParseInitializers),
                    (n, inits) => new NewNode { Location = n.Location, Initializers = inits }
                ),
                // "new" <type> <arguments> ("{" <initializers> "}")?
                ScoopParsers.Sequence(
                    new KeywordParser("new"),
                    Types,
                    ArgumentLists,
                    ScoopParsers.Optional(
                        new OldStyleRuleParser<ListNode<AstNode>>(ParseInitializers)
                    ),
                    (n, type, args, inits) => new NewNode { Location = n.Location, Type = type, Arguments = args, Initializers = inits as ListNode<AstNode> }
                ),
                // "new" <type> "{" <initializers> "}"
                ScoopParsers.Sequence(
                    new KeywordParser("new"),
                    Types,
                    new OldStyleRuleParser<ListNode<AstNode>>(ParseInitializers),
                    (n, type, inits) => new NewNode { Location = n.Location, Type = type, Initializers = inits }
                )
            );
            var terminal = ScoopParsers.First(
                // Terminal expression
                // Some of these "terminal" values may themselves be productions, but
                // are treated as a single value for the purposes of the expression parser
                // "true" | "false" | "null" | <Identifier> | <String> | <Number> | <new> | "(" <Expression> ")"
                new KeywordParser("true", "false", "null"),
                new IdentifierParser(),
                ScoopParsers.Token(TokenType.String, x => new StringNode(x)),
                ScoopParsers.Token(TokenType.Character, x => new CharNode(x)),
                ScoopParsers.Token(TokenType.Integer, x => new IntegerNode(x)),
                ScoopParsers.Token(TokenType.UInteger, x => new UIntegerNode(x)),
                ScoopParsers.Token(TokenType.Long, x => new LongNode(x)),
                ScoopParsers.Token(TokenType.ULong, x => new ULongNode(x)),
                ScoopParsers.Token(TokenType.Decimal, x => new DecimalNode(x)),
                ScoopParsers.Token(TokenType.Float, x => new FloatNode(x)),
                ScoopParsers.Token(TokenType.Double, x => new DoubleNode(x)),
                newParser,
                ScoopParsers.Sequence<OperatorNode, AstNode, OperatorNode, AstNode>(
                    new OperatorParser("("),
                    ScoopParsers.Deferred(() => ExpressionList),
                    new OperatorParser(")"),
                    (a, expr, b) => expr
                )
            );

            var current = terminal.Parse(t).GetResult();
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
                        memberAccessNode.GenericArguments = GenericTypeArguments.Parse(t).GetResult();

                    current = memberAccessNode;
                    continue;
                }

                // Invoke operator
                // <terminal> "(" <args> ")"
                if (lookahead.IsOperator("("))
                {
                    var args = ArgumentLists.Parse(t).GetResult();
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
                    var args = indexingParser.Parse(t).GetResult();
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

        private ListNode<AstNode> ParseInitializers(ITokenizer t)
        {
            // TODO: "[" <key> "]" "=" initializers may only occur after property initializers
            if (!t.NextIs(TokenType.Operator, "{", true))
                return null;
            var inits = new ListNode<AstNode>();
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
                        var init = Expressions.Parse(t).GetResult();
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
            var key = Expressions.Parse(t).GetResult();
            t.Expect(TokenType.Operator, ",");
            var value = Expressions.Parse(t).GetResult();
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
            var value = Expressions.Parse(t).GetResult();
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
            var value = Expressions.Parse(t).GetResult();
            return new PropertyInitializerNode
            {
                Location = propertyToken.Location,
                Property = new IdentifierNode(propertyToken),
                Value = value
            };
        }
    }
}

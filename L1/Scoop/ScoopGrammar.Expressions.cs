using System.Collections.Generic;
using System.Linq;
using Scoop.Parsers;
using Scoop.SyntaxTree;
using Scoop.Tokenization;
using static Scoop.Parsers.ScoopParsers;

namespace Scoop
{
    public partial class ScoopGrammar
    {
        private IParser<AstNode> _expressionConditional;

        public AstNode ParseExpression(string s) => Expressions.Parse(new Tokenizer(s)).GetResult();

        private void InitializeExpressions()
        {
            var argumentParser = First(
                Sequence(
                    new IdentifierParser(),
                    new OperatorParser(":"),
                    _requiredExpression,
                    (name, s, expr) => new NamedArgumentNode { Name = name, Separator = s, Value = expr }
                ),
                Expressions
            );
            ArgumentLists = First(
                Sequence(
                    new OperatorParser("("),
                    new OperatorParser(")"),
                    (a, b) => ListNode<AstNode>.Default()
                ),
                Sequence(
                    _requiredOpenParen,
                    SeparatedList(
                        argumentParser,
                        new OperatorParser(","),
                        items => new ListNode<AstNode> { Items = items.ToList(), Separator = new OperatorNode(",") }
                    ),
                    _requiredCloseParen,
                    (a, items, c) => items.WithUnused(a, c)
                )
            ).Named("ArgumentLists");

            // TODO: A proper precedence-based parser for expressions to try and save performance and stack space.
            var expressionPostfix = new OldStyleRuleParser<AstNode>(ParseExpressionPostfix);
            var expressionUnary = First(
                // TODO: We should loop and get multiple unary operators
                // ("-" | "+" | "~", etc) <Postfix>
                Sequence(
                    new OperatorParser("-", "+", "++", "--", "!", "~"),
                    expressionPostfix,
                    (op, expr) => new PrefixOperationNode
                    {
                        Location = op.Location,
                        Operator = op,
                        Right = expr
                    }
                ),
                // "await" | "throw" <postfix>
                Sequence(
                    Transform(
                        new KeywordParser("await", "throw"),
                        k => new OperatorNode
                        {
                            Location = k.Location,
                            Operator = k.Keyword
                        }
                    ),
                    expressionPostfix,
                    (op, expr) => new PrefixOperationNode
                    {
                        Location = op.Location,
                        Operator = op,
                        Right = expr
                    }
                ),
                // "(" <type> ")" <postfix>
                Sequence(
                    new OperatorParser("("),
                    Types,
                    new OperatorParser(")"),
                    expressionPostfix,
                    (o, type, c, expr) => new CastNode
                    {
                        Location = o.Location,
                        Type = type,
                        Right = expr
                    }.WithUnused(o, c)
                ),
                // <postfix>
                expressionPostfix
            );
            var expressionMultiplicative = Infix(
                // Operators with * / % precidence
                // <Unary> (<op> <Unary>)+
                expressionUnary,
                new OperatorParser("*", "/", "%"),
                expressionUnary,
                (left, op, right) => new InfixOperationNode
                {
                    Location = op.Location,
                    Left = left,
                    Operator = op,
                    Right = right
                }
            );
            var expressionAdditive = Infix(
                // Operators with + - precidence
                // <multiplicative> (<op> <multiplicative>)+
                expressionMultiplicative,
                new OperatorParser("+", "-"),
                expressionMultiplicative,
                (left, op, right) => new InfixOperationNode
                {
                    Location = op.Location,
                    Left = left,
                    Operator = op,
                    Right = right
                }
            );
            var expressionTypeCoerce = Sequence(
                // "as" and "is" operators, which don't chain and have a slightly different form
                // <additive> (<op> <additive> <ident>?)?
                expressionAdditive,
                Optional(
                    Sequence<OperatorNode, TypeNode, AstNode, AstNode>(
                        new OperatorParser("as", "is"),
                        Types,
                        Optional(_identifiers),
                        (op, type, name) => new TypeCoerceNode
                        {
                            Operator = op,
                            Type = type,
                            Alias = name as IdentifierNode
                        }
                    )
                ),
                ProduceTypeCoerce
            );
            var expressionEquality = Infix(
                // Equality/comparison operators
                // <typeCoerce> (<op> <typeCoerce>)+
                expressionTypeCoerce,
                new OperatorParser("==", "!=", ">=", "<=", "<", ">"),
                expressionTypeCoerce,
                (left, op, right) => new InfixOperationNode
                {
                    Location = op.Location,
                    Left = left,
                    Operator = op,
                    Right = right
                }
            );
            var expressionBitwise = Infix(
                // Bitwise operators
                // <equality> (<op> <equality>)+
                expressionEquality,
                new OperatorParser("&", "^", "|"),
                expressionEquality,
                (left, op, right) => new InfixOperationNode
                {
                    Location = op.Location,
                    Left = left,
                    Operator = op,
                    Right = right
                }
            );
            var expressionLogical = Infix(
                // Logical operators
                // <bitwise> (<op> <bitwise>)+
                expressionBitwise,
                new OperatorParser("&&", "||"),
                expressionBitwise,
                (left, op, right) => new InfixOperationNode
                {
                    Location = op.Location,
                    Left = left,
                    Operator = op,
                    Right = right
                }
            );
            var expressionCoalesce = Infix(
                // null-coalesce operator
                // <logical> (<op> <local>)+
                expressionLogical,
                new OperatorParser("??"),
                expressionLogical,
                (left, op, right) => new InfixOperationNode
                {
                    Location = op.Location,
                    Left = left,
                    Operator = op,
                    Right = right
                }
            );
            _expressionConditional = Sequence(
                // <expr> | <expr> "?" <expr> ":"
                expressionCoalesce,
                Optional(
                    Sequence(
                        new OperatorParser("?"),
                        Required(
                            Deferred(() => _expressionConditional),
                            () => new EmptyNode(),
                            Errors.MissingExpression
                        ),
                        new OperatorParser(":"),
                        Required(
                            Deferred(() => _expressionConditional),
                            () => new EmptyNode(),
                            Errors.MissingExpression
                        ),
                        (q, consequent, c, alternative) => new ConditionalNode
                        {
                            Location = q.Location,
                            IfTrue = consequent,
                            IfFalse = alternative
                        }.WithUnused(q, c)
                    )
                ),
                ProduceConditional
            );
            var expressionAssignment = Infix(
                // null-coalesce operator
                // <logical> (<op> <local>)+
                // TODO: assignment operators are right associative, so this rule does chained assignments backwards
                _expressionConditional,
                new OperatorParser("=", "+=", "-=", "/=", "%="),
                _expressionConditional,
                (left, op, right) => new InfixOperationNode
                {
                    Location = op.Location,
                    Left = left,
                    Operator = op,
                    Right = right
                }
            );

            var expressionLambda = First(
                // "(" ")" "=>" ( <expression> | "{" <methodBody> "}" )
                // <assignmentExpression>
                // <Identifier> "=>" ( <expression> | "{" <methodBody> "}" )
                // "(" <identifierList> ")"  "=>" ( <expression> | "{" <methodBody> "}" )
                Sequence(
                    First(
                        Transform(
                            new IdentifierParser(),
                            id => new ListNode<IdentifierNode> { Separator = new OperatorNode(","), [0] = id }
                        ),
                        Sequence(
                            new OperatorParser("("),
                            new OperatorParser(")"),
                            (a, b) => new ListNode<IdentifierNode> { Separator = new OperatorNode(","), Items = new List<IdentifierNode>() }
                        ),
                        Sequence(
                            new OperatorParser("("),
                            SeparatedList(
                                new IdentifierParser(),
                                new OperatorParser(","),
                                args => new ListNode<IdentifierNode> { Items = args.ToList(), Separator =  new OperatorNode(",") }
                            ),
                            new OperatorParser(")"),
                            (a, items, c) => new ListNode<IdentifierNode> { Separator = new OperatorNode(","), Items = items.ToList() }
                        )
                    ),
                    new OperatorParser("=>"),
                    First(
                        Deferred(() => NormalMethodBody),
                        Transform(
                            expressionAssignment,
                            body => new ListNode<AstNode> { [0] = body }
                        )
                    ),
                    (parameters, x, body) => new LambdaNode
                    {
                        Parameters = parameters,
                        Location = x.Location,
                        Statements = body
                    }.WithUnused(x)
                ),
                expressionAssignment
            );
            _expressions = expressionLambda;

            ExpressionList = SeparatedList(
                Expressions,
                new OperatorParser(","),
                items => new ListNode<AstNode> { Items = items.ToList(), Separator = new OperatorNode(",") },
                atLeastOne: true
            ).Named("ExpressionList");
        }

        private AstNode ProduceConditional(AstNode expr, AstNode rhs)
        {
            if (rhs is ConditionalNode conditional)
            {
                conditional.Condition = expr;
                return conditional;
            }

            return expr;
        }

        private AstNode ProduceTypeCoerce(AstNode left, AstNode rhs)
        {
            if (rhs is TypeCoerceNode coerce)
            {
                coerce.Left = left;
                return coerce;
            }

            return left;
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
            var indexingParser = Sequence(
                new OperatorParser("["),
                SeparatedList(
                    Expressions,
                    new OperatorParser(","),
                    items => new ListNode<AstNode> { Items = items.ToList(), Separator = new OperatorNode(",") }
                ),
                _requiredCloseBrace,
                (a, items, b) => items.WithUnused(a, b)
            );
            var newParser = First(
                // "new" "{" <initializers> "}"
                Sequence(
                    new KeywordParser("new"),
                    new OldStyleRuleParser<ListNode<AstNode>>(ParseInitializers),
                    (n, inits) => new NewNode { Location = n.Location, Initializers = inits }
                ),
                // "new" <type> <arguments> ("{" <initializers> "}")?
                Sequence(
                    new KeywordParser("new"),
                    Types,
                    ArgumentLists,
                    Optional(
                        new OldStyleRuleParser<ListNode<AstNode>>(ParseInitializers)
                    ),
                    (n, type, args, inits) => new NewNode { Location = n.Location, Type = type, Arguments = args, Initializers = inits as ListNode<AstNode> }
                ),
                // "new" <type> "{" <initializers> "}"
                Sequence(
                    new KeywordParser("new"),
                    Types,
                    new OldStyleRuleParser<ListNode<AstNode>>(ParseInitializers),
                    (n, type, inits) => new NewNode { Location = n.Location, Type = type, Initializers = inits }
                )
            );
            var terminal = First(
                // Terminal expression
                // Some of these "terminal" values may themselves be productions, but
                // are treated as a single value for the purposes of the expression parser
                // "true" | "false" | "null" | <Identifier> | <String> | <Number> | <new> | "(" <Expression> ")"
                new KeywordParser("true", "false", "null"),
                new IdentifierParser(),
                Token(TokenType.String, x => new StringNode(x)),
                Token(TokenType.Character, x => new CharNode(x)),
                Token(TokenType.Integer, x => new IntegerNode(x)),
                Token(TokenType.UInteger, x => new UIntegerNode(x)),
                Token(TokenType.Long, x => new LongNode(x)),
                Token(TokenType.ULong, x => new ULongNode(x)),
                Token(TokenType.Decimal, x => new DecimalNode(x)),
                Token(TokenType.Float, x => new FloatNode(x)),
                Token(TokenType.Double, x => new DoubleNode(x)),
                newParser,
                Sequence<OperatorNode, AstNode, OperatorNode, AstNode>(
                    new OperatorParser("("),
                    Deferred(() => ExpressionList),
                    new OperatorParser(")"),
                    (a, expr, b) => expr
                )
            );
            // TODO: This
            //var expressionPostfix = Sequence(
            //    terminal,
            //    List(
            //        // TODO: Need a way to pass the parsed terminal to the produce methods below?
            //        First<AstNode>(
            //            new OperatorParser("++", "--"),
            //            Sequence(
            //                new OperatorParser(".", "?."),
            //                _requiredIdentifier,
            //                (op, id) => new MemberAccessNode
            //                {
            //                    Location = op.Location,
            //                    IgnoreNulls = op.Operator == "?.",
            //                    MemberName = id
            //                }
            //            ),
            //            // TODO: Need to be able to tell these apart, they return the same type
            //            ArgumentLists,
            //            indexingParser
            //        ),
            //        operations => new ListNode<AstNode>()
            //    ),
            //    (term, ops) => new EmptyNode()
            //);

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
                        
                        Operator = new OperatorNode(lookahead),
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
            // TODO: Need to really understand initializer syntax better
            // TODO: "[" <key> "]" "=" initializers may only occur after property initializers
            if (!t.NextIs(TokenType.Operator, "{", true))
                return null;
            var inits = new ListNode<AstNode>();
            while (true)
            {
                // TODO: We can either have expression initializers (for lists) or we can have other initializers in any order
                var lookaheads = t.Peek(2);
                if (lookaheads[0].IsOperator("{"))
                {
                    // TODO: This initializer corresponds to a .Add method call, and can have as many arguments as .Add has
                    // (not just 2, and not just for Dictionary types
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

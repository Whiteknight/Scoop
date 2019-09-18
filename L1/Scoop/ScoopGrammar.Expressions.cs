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
        private IParser<AstNode> _expressionUnary;

        private void InitializeExpressions()
        {
            var arguments = First(
                Sequence(
                    new IdentifierParser(),
                    new OperatorParser(":"),
                    _requiredExpression,
                    (name, s, expr) => new NamedArgumentNode { Name = name, Separator = s, Value = expr }
                ),
                Expressions
            ).Named("arguments");

            ArgumentLists = First(
                // A required argument list
                // "(" <commaSeparatedArgs>? ")"
                Sequence(
                    new OperatorParser("("),
                    new OperatorParser(")"),
                    (a, b) => ListNode<AstNode>.Default()
                ),
                Sequence(
                    _requiredOpenParen,
                    SeparatedList(
                        arguments,
                        new OperatorParser(","),
                        items => new ListNode<AstNode>
                        {
                            Items = items.ToList(),
                            Separator = new OperatorNode(",")
                        }
                    ),
                    _requiredCloseParen,
                    (a, items, c) => items.WithUnused(a, c)
                )
            ).Named("ArgumentLists");
            var maybeArgumentList = First(
                // An optional argument list, is able to fail without diagnostics
                // "(" <commaSeparatedArgs>? ")"
                Sequence(
                    new OperatorParser("("),
                    new OperatorParser(")"),
                    (a, b) => ListNode<AstNode>.Default()
                ),
                Sequence(
                    new OperatorParser("("),
                    SeparatedList(
                        arguments,
                        new OperatorParser(","),
                        items => new ListNode<AstNode>
                        {
                            Items = items.ToList(),
                            Separator = new OperatorNode(",")
                        }
                    ),
                    _requiredCloseParen,
                    (a, items, c) => items.WithUnused(a, c)
                )
            ).Named("maybeArgumentLists");

            // TODO: A proper precedence-based parser for expressions to try and save performance and stack space.
            var indexers = Sequence(
                new OperatorParser("["),
                SeparatedList(
                    Expressions,
                    new OperatorParser(","),
                    items => new ListNode<AstNode>
                    {
                        Items = items.ToList(),
                        Separator = new OperatorNode(",")
                    }
                ),
                _requiredCloseBrace,
                (a, items, b) => items.WithUnused(a, b)
            ).Named("indexers");

            var nonCollectionInitializer = First<AstNode>(
                Sequence(
                    // <ident> "=" <Expression>
                    _identifiers,
                    new OperatorParser("="),
                    Expressions,
                    (name, e, expr) => new PropertyInitializerNode
                    {
                        Location = name.Location,
                        Property = name,
                        Value = expr
                    }.WithUnused(e)
                ),
                Sequence(
                    // "[" <args> "]" "=" <Expression>
                    new OperatorParser("["),
                    SeparatedList(
                        Expressions,
                        new OperatorParser(","),
                        items => new ListNode<AstNode>
                        {
                            Items = items.ToList(),
                            Separator = new OperatorNode(",")
                        }
                    ),
                    new OperatorParser("]"),
                    new OperatorParser("="),
                    Expressions,
                    (o, args, c, e, value) => new IndexerInitializerNode
                    {
                        Location = o.Location,
                        Arguments = args,
                        Value = value
                    }.WithUnused(o, c, e)
                ),
                Sequence(
                    // "{" <args> "}" 
                    new OperatorParser("{"),
                    SeparatedList(
                        Expressions,
                        new OperatorParser(","),
                        items => new ListNode<AstNode>
                        {
                            Items = items.ToList(),
                            Separator = new OperatorNode(",")
                        }
                    ),
                    new OperatorParser("}"),
                    (o, args, c) => new AddInitializerNode
                    {
                        Location = o.Location,
                        Arguments = args
                    }.WithUnused(o, c)
                )
            );
            var initializers = Sequence(
                new OperatorParser("{"),
                First(
                    SeparatedList(
                        nonCollectionInitializer,
                        new OperatorParser(","),
                        items => new ListNode<AstNode>
                        {
                            Items = items.ToList(),
                            Separator = new OperatorNode(",")
                        },
                        atLeastOne: true
                    ),
                    SeparatedList(
                        Expressions,
                        new OperatorParser(","),
                        items => new ListNode<AstNode>
                        {
                            Items = items.ToList(),
                            Separator = new OperatorNode(",")
                        }
                    ).Named("initializers.Collection"),
                    Produce(() => ListNode<AstNode>.Default()).Named("initializers.Empty")
                ),
                new OperatorParser("}"),
                (a, inits, b) => inits.WithUnused(a, b)
            );
            var newParser = First(
                // "new" "{" <initializers> "}"
                Sequence(
                    new KeywordParser("new"),
                    initializers,
                    (n, inits) => new NewNode
                    {
                        Location = n.Location,
                        Initializers = inits
                    }
                ),
                // "new" <type> "{" <initializers> "}"
                Sequence(
                    new KeywordParser("new"),
                    Types,
                    initializers,
                    (n, type, inits) => new NewNode
                    {
                        Location = n.Location,
                        Type = type,
                        Initializers = inits
                    }
                ),
                // "new" <type> <arguments> ("{" <initializers> "}")?
                Sequence(
                    new KeywordParser("new"),
                    Types,
                    ArgumentLists,
                    Optional(initializers),
                    (n, type, args, inits) => new NewNode
                    {
                        Location = n.Location,
                        Type = type,
                        Arguments = args,
                        Initializers = inits as ListNode<AstNode>
                    }
                )
            ).Named("new");

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
                    _requiredCloseParen,
                    (a, expr, b) => expr.WithUnused(a, b)
                )
            ).Named("terminal");

            var expressionPostfix = ApplyPostfix(
                terminal,
                init => First<AstNode>(
                    Sequence(
                        // <terminal> ("++" | "--")
                        init,
                        new OperatorParser("++", "--"),
                        (left, op) => new PostfixOperationNode
                        {
                            Location = left.Location,
                            Left = left,
                            Operator = op
                        }
                    ).Named("postfix.Increment"),
                    Sequence(
                        // Method Invoke
                        // <terminal> ("." | "?.") <identifier> <GenericTypeArgs>? <ArgumentList>
                        init,
                        new OperatorParser(".", "?."),
                        _identifiers,
                        _optionalGenericTypeArguments,
                        maybeArgumentList,
                        (left, op, name, genArgs, args) => new InvokeNode
                        {
                            Location = args.Location,
                            Instance = new MemberAccessNode
                            {
                                Location = left.Location,
                                Instance = left,
                                Operator = op,
                                MemberName = name,
                                GenericArguments = genArgs as ListNode<TypeNode>
                            },
                            Arguments = args
                        }
                    ).Named("postfix.MethodInvoke"),
                    Sequence(
                        // property access
                        // <terminal> ("." | "?.") <identifier>
                        init,
                        new OperatorParser(".", "?."),
                        _requiredIdentifier,
                        (left, op, name) => new MemberAccessNode
                        {
                            Location = left.Location,
                            Instance = left,
                            Operator = op,
                            MemberName = name
                        }
                    ).Named("postfix.PropertyAccess"),
                    Sequence(
                        // Invoke operator
                        // <terminal> "(" <args> ")"
                        init,
                        maybeArgumentList,
                        (left, args) => new InvokeNode
                        {
                            Location = left.Location,
                            Instance = left,
                            Arguments = args
                        }
                    ).Named("postfix.Invoke"),
                    Sequence(
                        // Index operator
                        // <terminal> "[" <args> "]"
                        init,
                        indexers,
                        (left, args) => new IndexNode
                        {
                            Location = left.Location,
                            Instance = left,
                            Arguments = args
                        }
                    ).Named("postfix.Index")
                )
            ).Named("postfix");

            var expressionUnary = First(
                // ("-" | "+" | "~", etc) <Unary>
                Sequence(
                    new OperatorParser("-", "+", "++", "--", "!", "~"),
                    Deferred(() => _expressionUnary),
                    (op, expr) => new PrefixOperationNode
                    {
                        Location = op.Location,
                        Operator = op,
                        Right = expr
                    }
                ),
                // "await" | "throw" <Unary>
                Sequence(
                    Transform(
                        new KeywordParser("await", "throw"),
                        k => new OperatorNode
                        {
                            Location = k.Location,
                            Operator = k.Keyword
                        }
                    ),
                    Deferred(() => _expressionUnary),
                    (op, expr) => new PrefixOperationNode
                    {
                        Location = op.Location,
                        Operator = op,
                        Right = expr
                    }
                ),
                // "(" <type> ")" <Unary>
                Sequence(
                    new OperatorParser("("),
                    Types,
                    new OperatorParser(")"),
                    Deferred(() => _expressionUnary),
                    (o, type, c, expr) => new CastNode
                    {
                        Location = o.Location,
                        Type = type,
                        Right = expr
                    }.WithUnused(o, c)
                ),
                // <postfix>
                expressionPostfix
            ).Named("unary");
            _expressionUnary = expressionUnary;

            var expressionMultiplicative = Infix(
                // Operators with * / % precidence
                // <Unary> (<op> <Unary>)+
                expressionUnary,
                new OperatorParser("*", "/", "%"),
                Required(expressionUnary, () => new EmptyNode(), Errors.MissingExpression),
                (left, op, right) => new InfixOperationNode
                {
                    Location = op.Location,
                    Left = left,
                    Operator = op,
                    Right = right
                }
            ).Named("multiplicative");

            var expressionAdditive = Infix(
                // Operators with + - precidence
                // <multiplicative> (<op> <multiplicative>)+
                expressionMultiplicative,
                new OperatorParser("+", "-"),
                Required(expressionMultiplicative, () => new EmptyNode(), Errors.MissingExpression),
                (left, op, right) => new InfixOperationNode
                {
                    Location = op.Location,
                    Left = left,
                    Operator = op,
                    Right = right
                }
            ).Named("additive");

            var expressionTypeCoerce = ApplyPostfix(
                // "as" and "is" operators, which don't chain and have a slightly different form
                // <additive> (<op> <additive> <ident>?)?
                expressionAdditive,
                additive =>
                    Sequence(
                        additive,
                        new OperatorParser("as", "is"),
                        _requiredType,
                        Optional(_identifiers),
                        (left, op, type, name) => new TypeCoerceNode
                        {
                            Left = left,
                            Operator = op,
                            Type = type,
                            Alias = name as IdentifierNode
                        }
                    )
            ).Named("typeCoerce");

            var expressionEquality = Infix(
                // Equality/comparison operators
                // <typeCoerce> (<op> <typeCoerce>)+
                expressionTypeCoerce,
                new OperatorParser("==", "!=", ">=", "<=", "<", ">"),
                Required(expressionTypeCoerce, () => new EmptyNode(), Errors.MissingExpression),
                (left, op, right) => new InfixOperationNode
                {
                    Location = op.Location,
                    Left = left,
                    Operator = op,
                    Right = right
                }
            ).Named("equality");

            var expressionBitwise = Infix(
                // Bitwise operators
                // <equality> (<op> <equality>)+
                expressionEquality,
                new OperatorParser("&", "^", "|"),
                Required(expressionEquality, () => new EmptyNode(), Errors.MissingExpression),
                (left, op, right) => new InfixOperationNode
                {
                    Location = op.Location,
                    Left = left,
                    Operator = op,
                    Right = right
                }
            ).Named("bitwise");

            var expressionLogical = Infix(
                // Logical operators
                // <bitwise> (<op> <bitwise>)+
                expressionBitwise,
                new OperatorParser("&&", "||"),
                Required(expressionBitwise, () => new EmptyNode(), Errors.MissingExpression),
                (left, op, right) => new InfixOperationNode
                {
                    Location = op.Location,
                    Left = left,
                    Operator = op,
                    Right = right
                }
            ).Named("logical");

            var expressionCoalesce = Infix(
                // null-coalesce operator
                // <logical> (<op> <local>)+
                expressionLogical,
                new OperatorParser("??"),
                Required(expressionLogical, () => new EmptyNode(), Errors.MissingExpression),
                (left, op, right) => new InfixOperationNode
                {
                    Location = op.Location,
                    Left = left,
                    Operator = op,
                    Right = right
                }
            ).Named("coalesce");

            _expressionConditional = ApplyPostfix(
                // <expr> | <expr> "?" <expr> ":"
                expressionCoalesce,
                init =>
                    Sequence(
                        init,
                        new OperatorParser("?"),
                        Required(
                            Deferred(() => _expressionConditional),
                            () => new EmptyNode(),
                            Errors.MissingExpression
                        ),
                        _requiredColon,
                        Required(
                            Deferred(() => _expressionConditional),
                            () => new EmptyNode(),
                            Errors.MissingExpression
                        ),
                        (condition, q, consequent, c, alternative) => new ConditionalNode
                        {
                            Location = q.Location,
                            Condition = condition,
                            IfTrue = consequent,
                            IfFalse = alternative
                        }.WithUnused(q, c)
                    )
            ).Named("conditional");

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
            ).Named("assignment");

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
                                args => new ListNode<IdentifierNode> { Items = args.ToList(), Separator = new OperatorNode(",") }
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
                            // TODO: Does this need to become explicit "return"?
                            // Make sure the transpiler deals with this correctly if not
                            body => new ListNode<AstNode> { [0] = body }
                        ),
                        Error<ListNode<AstNode>>(false, Errors.MissingExpression)
                    ),
                    (parameters, x, body) => new LambdaNode
                    {
                        Parameters = parameters,
                        Location = x.Location,
                        Statements = body
                    }.WithUnused(x)
                ),
                expressionAssignment
            ).Named("lambda");
            _expressions = expressionLambda;

            ExpressionList = SeparatedList(
                Expressions,
                new OperatorParser(","),
                items => new ListNode<AstNode> { Items = items.ToList(), Separator = new OperatorNode(",") },
                atLeastOne: true
            ).Named("ExpressionList");
        }
    }
}

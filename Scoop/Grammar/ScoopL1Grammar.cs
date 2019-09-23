using System.Collections.Generic;
using System.Linq;
using Scoop.Parsers;
using Scoop.SyntaxTree;
using Scoop.Tokenization;
using static Scoop.Parsers.ScoopParsers;

namespace Scoop.Grammar
{
    public class ScoopL1Grammar : IScoopGrammar
    {
        public ScoopL1Grammar()
        {
            Initialize();
        }

        public IParser<CompilationUnitNode> CompilationUnits { get; private set; }
        public IParser<TypeNode> Types { get; private set; }
        public IParser<AstNode> Expressions { get; private set; }
        public IParser<ListNode<AstNode>> ExpressionList { get; private set; }
        public IParser<AstNode> Statements { get; private set; }
        public IParser<ListNode<AttributeNode>> Attributes { get; set; }
        public IParser<DelegateNode> Delegates { get; set; }
        public IParser<EnumNode> Enums { get; set; }
        public IParser<ClassNode> Classes { get; set; }
        public IParser<AstNode> ClassMembers { get; set; }
        public IParser<InterfaceNode> Interfaces { get; set; }


        private IParser<AstNode> _accessModifiers;
        private IParser<IdentifierNode> _identifiers;
        private IParser<OperatorNode> _requiredSemicolon;
        private IParser<OperatorNode> _requiredOpenBracket;
        private IParser<OperatorNode> _requiredCloseBracket;
        private IParser<OperatorNode> _requiredOpenParen;
        private IParser<OperatorNode> _requiredCloseParen;
        private IParser<OperatorNode> _requiredColon;
        private IParser<OperatorNode> _requiredCloseBrace;
        private IParser<OperatorNode> _requiredCloseAngle;
        private IParser<OperatorNode> _requiredEquals;
        private IParser<IdentifierNode> _requiredIdentifier;
        private IParser<TypeNode> _requiredType;
        private IParser<TypeNode> _declareTypes;
        private IParser<ListNode<IdentifierNode>> _genericTypeParameters;
        private IParser<ListNode<AstNode>> _argumentLists;
        private IParser<ListNode<AstNode>> _maybeArgumentLists;
        private IParser<AstNode> _requiredExpression;
        private IParser<AstNode> _optionalGenericTypeArguments;
        private IParser<ListNode<ParameterNode>> _parameterLists;
        private IParser<ListNode<TypeConstraintNode>> _typeConstraints;
        private IParser<ListNode<AstNode>> _normalMethodBody;
        private IParser<NewNode> _newParser;
        private IParser<DottedIdentifierNode> _dottedIdentifiers;
        private IParser<AstNode> _expressions;
        private IParser<AstNode> _expressionConditional;
        private IParser<AstNode> _expressionUnary;

        private void Initialize()
        {
            // Setup some parsers by reference to avoid circular references
            Expressions = Deferred(() => _expressions).Named("Expressions");

            // Setup some commonly-used parsers
            _identifiers = new IdentifierParser().Named("_identifiers");
            _accessModifiers = Optional(
                new KeywordParser("public", "private")
            ).Named("accessModifiers");

            // Setup some parsers for requiring operators or communicating helpful errors
            _requiredSemicolon = Required(new OperatorParser(";"), Errors.MissingSemicolon);
            _requiredOpenBracket = Required(new OperatorParser("{"), Errors.MissingOpenBracket);
            _requiredCloseBracket = Required(new OperatorParser("}"), Errors.MissingCloseBracket);
            _requiredOpenParen = Required(new OperatorParser("("), Errors.MissingOpenParen);
            _requiredCloseParen = Required(new OperatorParser(")"), Errors.MissingCloseParen);
            _requiredColon = Required(new OperatorParser(":"), Errors.MissingColon);
            _requiredCloseBrace = Required(new OperatorParser("]"), Errors.MissingCloseBrace);
            _requiredCloseAngle = Required(new OperatorParser(">"), Errors.MissingCloseAngle);
            _requiredIdentifier = Required(new IdentifierParser(), Errors.MissingIdentifier);
            _requiredEquals = Required(new OperatorParser("="), Errors.MissingEquals);

            // Parsers to require certain productions or add a helpful error
            _requiredType = Required(Types, Errors.MissingType);
            _requiredExpression = Required(Expressions, () => new EmptyNode(), Errors.MissingExpression);

            // Setup individual sections of the grammar
            InitializeTopLevel();
            InitializeTypes();
            InitializeDeclareTypes();
            InitializeGenericTypeArguments();
            InitializeGenericTypeParameters();
            InitializeTypeConstraints();
            InitializeArgumentLists();
            InitializeNew();
            InitializeExpressions();
            InitializeStatements();
            InitializeAttributes();
            InitializeParameters();
            InitializeDelegates();
            InitializeEnums();
            InitializeClasses();
        }

        private void InitializeAttributes()
        {
            var argumentParser = First(
                // (<identifier> "=" <expr>) | <expr>
                Sequence(
                    new IdentifierParser(),
                    new OperatorParser("="),
                    // TODO: I think these expressions can only be terminals or member accesses (consts or enums, etc)
                    Expressions,
                    (name, s, expr) => new NamedArgumentNode { Name = name, Separator = s, Value = expr }
                ),
                // TODO: I think these expressions can only be terminals or member accesses (consts or enums, etc)
                Expressions
            ).Named("attributeArgument");
            var argumentListParser = Optional(
                // (("(" ")") | ("(" <argumentList> ")"))?
                First(
                    Sequence(
                        new OperatorParser("("),
                        new OperatorParser(")"),
                        (a, b) => ListNode<AstNode>.Default()
                    ),
                    Sequence(
                        new OperatorParser("("),
                        SeparatedList(
                            argumentParser,
                            new OperatorParser(","),
                            items => new ListNode<AstNode> { Items = items.ToList(), Separator = new OperatorNode(",") }
                        ),
                        _requiredCloseParen,
                        (a, items, c) => items.WithUnused(a, c)
                    )
                ).Named("attributeArgumentList")
            );
            var attrParser = SeparatedList(
                Sequence(
                    // (<keyword> ":")? <type> <argumentList>
                    Optional(
                        Sequence(
                            new KeywordParser(),
                            _requiredColon,
                            (target, o) => target.WithUnused(o)
                        )
                    ),
                    Types,
                    argumentListParser,
                    (target, type, args) => new AttributeNode
                    {
                        Location = type.Location,
                        Target = target as KeywordNode,
                        Type = type,
                        Arguments = args as ListNode<AstNode>
                    }
                ).Named("attribute"),
                new OperatorParser(","),
                list => new ListNode<AttributeNode> { Items = list.ToList(), Separator = new OperatorNode(",") }
            ).Named("attributeList");
            Attributes = Transform(
                Optional(
                    List(
                        Sequence(
                            // "[" <attributeList> "]"
                            new OperatorParser("["),
                            attrParser,
                            _requiredCloseBrace,
                            (a, attrs, b) => attrs.WithUnused(a, b)
                        ),
                        list => new ListNode<AttributeNode> { Items = list.SelectMany(l => l.Items).ToList() }.WithUnused(list.SelectMany(a => a.Unused.OrEmptyIfNull()).ToArray())
                    ).Named("attributeTagList")
                ),
                n => n is EmptyNode ? ListNode<AttributeNode>.Default() : n as ListNode<AttributeNode>
            );
        }

        private void InitializeTopLevel()
        {
            _dottedIdentifiers = Transform(
                SeparatedList(
                    _identifiers,
                    new OperatorParser("."),
                    items => new ListNode<IdentifierNode> { Items = items.ToList(), Separator = new OperatorNode(".") },
                    atLeastOne: true
                ),
                items => new DottedIdentifierNode(items.Select(i => i.Id), items.Location)
            );
            // "using" <namespaceName> ";"
            var parseUsingDirective = Sequence(
                new KeywordParser("using"),
                Required(_dottedIdentifiers, () => new DottedIdentifierNode(""), Errors.MissingNamespaceName),
                _requiredSemicolon,
                (a, b, c) => new UsingDirectiveNode
                {
                    Location = a.Location,
                    Namespace = b
                }.WithUnused(a, c)
            );

            var namespaceMembers = First<AstNode>(
                Token(TokenType.CSharpLiteral, t => new CSharpNode(t)),
                Deferred(() => Classes),
                Deferred(() => Interfaces),
                Deferred(() => Enums),
                Deferred(() => Delegates)
            );
            var namespaceBody = First(
                Sequence(
                    new OperatorParser("{"),
                    new OperatorParser("}"),
                    (a, b) => new ListNode<AstNode>()
                ),
                Sequence(
                    new OperatorParser("{"),
                    List(
                        namespaceMembers,
                        members => new ListNode<AstNode> { Items = members.ToList() }
                    ),
                    _requiredCloseBracket,
                    (a, members, b) => members.WithUnused(a, b)
                ),
                Error<ListNode<AstNode>>(false, Errors.MissingOpenBracket)
            );
            var parseNamespace = Sequence(
                // TODO: assembly-level attributes
                new KeywordParser("namespace"),
                Required(_dottedIdentifiers, () => new DottedIdentifierNode(""), Errors.MissingNamespaceName),
                namespaceBody,
                (ns, name, members) => new NamespaceNode
                {
                    Location = ns.Location,
                    Name = name,
                    Declarations = members
                }.WithUnused(ns)
            );
            CompilationUnits = Transform(
                List(
                    First<AstNode>(
                        parseUsingDirective,
                        parseNamespace
                    ),
                    items => new ListNode<AstNode> { Items = items.ToList() }
                ),
                list => new CompilationUnitNode
                {
                    Members = list
                }
            );
        }

        private void InitializeEnums()
        {
            var enumMember = Sequence(
                Attributes,
                new IdentifierParser(),
                Optional(
                    Sequence(
                        new OperatorParser("="),
                        _requiredExpression,
                        (e, expr) => expr.WithUnused(e)
                    )
                ),
                (attrs, name, value) => new EnumMemberNode
                {
                    Attributes = attrs.IsNullOrEmpty() ? null : attrs,
                    Location = name.Location,
                    Name = name,
                    Value = value is EmptyNode ? null : value
                }
            ).Named("enumMember");

            Enums = Sequence(
                Attributes,
                _accessModifiers,
                new KeywordParser("enum"),
                _requiredIdentifier,
                _requiredOpenBracket,
                SeparatedList(
                    enumMember,
                    new OperatorParser(","),
                    members => new ListNode<EnumMemberNode> { Items = members.ToList(), Separator = new OperatorNode(",") }
                ),
                _requiredCloseBracket,
                (attrs, vis, e, name, x, members, y) => new EnumNode
                {
                    Location = e.Location,
                    Attributes = attrs.IsNullOrEmpty() ? null : attrs,
                    AccessModifier = vis as KeywordNode,
                    Name = name,
                    Members = members
                }.WithUnused(e, x, y)
            ).Named("Enums");
        }

        private void InitializeDelegates()
        {
            Delegates = Sequence(
                // <attributes> <accessModifier>? "delegate" <type> <identifier> <genericParameters>? <parameters> <typeConstraints> ";"
                Deferred(() => Attributes),
                _accessModifiers,
                new KeywordParser("delegate"),
                _requiredType,
                _requiredIdentifier,
                _genericTypeParameters,
                _parameterLists,
                _typeConstraints,
                _requiredSemicolon,
                (attrs, vis, d, retType, name, gen, param, cons, s) => new DelegateNode
                {
                    Location = d.Location,
                    Attributes = attrs.IsNullOrEmpty() ? null : attrs,
                    AccessModifier = vis as KeywordNode,
                    ReturnType = retType,
                    Name = name,
                    GenericTypeParameters = gen.IsNullOrEmpty() ? null : gen,
                    Parameters = param,
                    TypeConstraints = cons.IsNullOrEmpty() ? null : cons
                }.WithUnused(d, s)
            ).Named("Delegates");
        }

        private void InitializeClasses()
        {
            var inheritanceList = Optional(
                // ":" <commaSeparatedType+>
                Sequence(
                    new OperatorParser(":"),
                    Required(
                        SeparatedList(
                            Types,
                            new OperatorParser(","),
                            types => new ListNode<TypeNode> { Items = types.ToList(), Separator = new OperatorNode(",") },
                            atLeastOne: true
                        ),
                        Errors.MissingType
                    ),
                    (colon, types) => types.WithUnused(colon))
            ).Named("inheritanceList");

            var interfaceMember = Sequence(
                Types,
                _requiredIdentifier,
                _genericTypeParameters,
                _parameterLists,
                _typeConstraints,
                _requiredSemicolon,
                (ret, name, genParm, parm, cons, s) => new MethodDeclareNode
                {
                    Location = name.Location,
                    ReturnType = ret,
                    Name = name,
                    GenericTypeParameters = genParm.IsNullOrEmpty() ? null : genParm,
                    Parameters = parm,
                    TypeConstraints = cons.IsNullOrEmpty() ? null : cons,
                }.WithUnused(s)
            ).Named("interfaceMember");

            var interfaceBody = First(
                Sequence(
                    new OperatorParser("{"),
                    new OperatorParser("}"),
                    (a, b) => new ListNode<MethodDeclareNode>().WithUnused(a, b)
                ),
                Sequence(
                    new OperatorParser("{"),
                    List(
                        interfaceMember,
                        members => new ListNode<MethodDeclareNode> { Items = members.ToList() }
                    ),
                    _requiredCloseBracket,
                    (a, members, b) => members.WithUnused(a, b)
                ),
                Error<ListNode<MethodDeclareNode>>(false, Errors.MissingOpenBracket)
            ).Named("interfaceBody");

            Interfaces = Sequence(
                Attributes,
                _accessModifiers,
                new KeywordParser("interface"),
                _requiredIdentifier,
                _genericTypeParameters,
                inheritanceList,
                _typeConstraints,
                interfaceBody,
                (attrs, vis, i, name, genParm, inh, cons, body) => new InterfaceNode
                {
                    Location = name.Location,
                    Attributes = attrs.IsNullOrEmpty() ? null : attrs,
                    AccessModifier = vis as KeywordNode,
                    Name = name,
                    GenericTypeParameters = genParm.IsNullOrEmpty() ? null : genParm,
                    Interfaces = inh as ListNode<TypeNode>,
                    TypeConstraints = cons.IsNullOrEmpty() ? null : cons,
                    Members = body
                }.WithUnused(i)
            ).Named("Interfaces");

            var constants = Sequence(
                _accessModifiers,
                new KeywordParser("const"),
                _requiredType,
                _requiredIdentifier,
                _requiredEquals,
                _requiredExpression,
                _requiredSemicolon,
                (vis, c, type, name, e, expr, s) => new ConstNode
                {
                    AccessModifier = vis as KeywordNode,
                    Location = name.Location,
                    Type = type,
                    Name = name,
                    Value = expr
                }.WithUnused(c, e, s)
            ).Named("constants");

            var exprMethodBody = Sequence(
                new OperatorParser("=>"),
                First(
                    Sequence(
                        new OperatorParser("{"),
                        new OperatorParser("}"),
                        (a, b) => new ListNode<AstNode>().WithUnused(a, b)
                    ),
                    Sequence(
                        new OperatorParser("{"),
                        List(
                            Statements,
                            stmts => new ListNode<AstNode> { Items = stmts.ToList() }
                        ),
                        _requiredCloseBracket,
                        (a, stmts, b) => stmts.WithUnused(a, b)
                    ),
                    Sequence(
                        Expressions,
                        _requiredSemicolon,
                        (expr, s) => new ListNode<AstNode> { new ReturnNode { Expression = expr } }.WithUnused(s)
                    ),
                    Error<ListNode<AstNode>>(false, Errors.MissingOpenBracket)
                ),
                (lambda, body) => body.WithUnused(lambda)
            ).Named("exprMethodBody");

            _normalMethodBody = First(
                Sequence(
                    new OperatorParser("{"),
                    new OperatorParser("}"),
                    (a, b) => new ListNode<AstNode>().WithUnused(a, b)
                ),
                Sequence(
                    new OperatorParser("{"),
                    List(
                        Statements,
                        stmts => new ListNode<AstNode> { Items = stmts.ToList() }
                    ),
                    _requiredCloseBracket,
                    (a, body, b) => body.WithUnused(a, b)
                )
            ).Named("NormalMethodBody");

            var methodBody = First(
                exprMethodBody,
                _normalMethodBody,
                Error<ListNode<AstNode>>(false, Errors.MissingOpenBracket)
            ).Named("methodBody");

            var constructors = Sequence(
                Attributes,
                _accessModifiers,
                _identifiers,
                _parameterLists,
                Optional(
                    Sequence(
                        new OperatorParser(":"),
                        Required(new IdentifierParser("this"), Errors.MissingThis),
                        _argumentLists,
                        (a, b, args) => args.WithUnused(a, b)
                    )
                ).Named("thisArgs"),
                methodBody,
                (attrs, vis, name, param, targs, body) => new ConstructorNode
                {
                    Attributes = attrs.IsNullOrEmpty() ? null : attrs,
                    Location = name.Location,
                    AccessModifier = vis as KeywordNode,
                    ClassName = name,
                    Parameters = param,
                    ThisArgs = targs as ListNode<AstNode>,
                    Statements = body
                }
            ).Named("constructors");

            var methods = Sequence(
                // <accessModifier>? "async"? <type> <ident> <genericTypeParameters>? <parameterList> <typeConstraints>? <methodBody>
                Attributes,
                _accessModifiers,
                // TODO: If we see "async" it must be a method and we should require everything else. No backtracking after that
                Optional(new KeywordParser("async")),
                Types,
                _identifiers,
                _genericTypeParameters,
                // TODO: Once we see the parameter list, it must be a method and we should not backtrack
                _parameterLists,
                _typeConstraints,
                methodBody,
                (attrs, vis, isAsync, retType, name, genParam, param, cons, body) => new MethodNode
                {
                    Location = name.Location,
                    Attributes = attrs.IsNullOrEmpty() ? null : attrs,
                    AccessModifier = vis as KeywordNode,
                    Modifiers = isAsync is EmptyNode ? null : new ListNode<KeywordNode> { isAsync as KeywordNode },
                    ReturnType = retType,
                    Name = name,
                    GenericTypeParameters = genParam.IsNullOrEmpty() ? null : genParam,
                    Parameters = param,
                    TypeConstraints = cons.IsNullOrEmpty() ? null : cons,
                    Statements = body
                }
            ).Named("methods");

            var fields = Sequence(
                Attributes,
                Types,
                _identifiers,
                _requiredSemicolon,
                (attrs, type, name, s) => new FieldNode
                {
                    Attributes = attrs.IsNullOrEmpty() ? null : attrs,
                    Location = name.Location,
                    Type = type,
                    Name = name
                }.WithUnused(s)
            ).Named("fields");

            ClassMembers = First<AstNode>(
                Token(TokenType.CSharpLiteral, cs => new CSharpNode(cs)),
                Deferred(() => Classes),
                Interfaces,
                Enums,
                Delegates,
                constants,
                fields,
                methods,
                constructors
            ).Named("ClassMembers");

            var classBody = First(
                Sequence(
                    new OperatorParser("{"),
                    new OperatorParser("}"),
                    (a, b) => new ListNode<AstNode>().WithUnused(a, b)
                ).Named("classBody.EmptyBrackets"),
                Sequence(
                    new OperatorParser("{"),
                    List(
                        ClassMembers,
                        members => new ListNode<AstNode> { Items = members.ToList() }
                    ),
                    _requiredCloseBracket,
                    (a, members, b) => members.WithUnused(a, b)
                ).Named("classBody.body"),
                Error<ListNode<AstNode>>(true, Errors.MissingOpenBracket)
            ).Named("classBody");

            Classes = Sequence(
                Attributes,
                _accessModifiers,
                Optional(new KeywordParser("partial")),
                new KeywordParser("class", "struct"),
                _requiredIdentifier,
                _genericTypeParameters,
                inheritanceList,
                _typeConstraints,
                classBody,
                (attrs, vis, isPartial, obj, name, genParm, contracts, cons, body) => new ClassNode
                {
                    Attributes = attrs.IsNullOrEmpty() ? null : attrs,
                    AccessModifier = vis as KeywordNode,
                    Modifiers = isPartial is KeywordNode k ? new ListNode<KeywordNode> { k } : null,
                    Type = obj,
                    Name = name,
                    GenericTypeParameters = genParm.IsNullOrEmpty() ? null : genParm,
                    Interfaces = contracts as ListNode<TypeNode>,
                    TypeConstraints = cons.IsNullOrEmpty() ? null : cons,
                    Members = body
                }
            ).Named("Classes");
        }

        private void InitializeParameters()
        {
            var parameter = Sequence(
                // <attributes> "params"? <type> <ident> ("=" <expr>)?
                Attributes,
                Optional(new KeywordParser("params")),
                Types,
                _requiredIdentifier,
                Optional(
                    Sequence(
                        new OperatorParser("="),
                        _requiredExpression,
                        (op, expr) => expr.WithUnused(op)
                    )
                ),
                (attrs, isparams, type, name, value) => new ParameterNode
                {
                    Location = type.Location,
                    Attributes = attrs.IsNullOrEmpty() ? null : attrs,
                    IsParams = isparams is KeywordNode,
                    Type = type,
                    Name = name,
                    DefaultValue = value is EmptyNode ? null : value
                }
            ).Named("parameter");

            _parameterLists = First(
                //("(" ")") | ("(" <commaSeparatedParameterList> ")")
                Sequence(
                    new OperatorParser("("),
                    new OperatorParser(")"),
                    (a, b) => ListNode<ParameterNode>.Default().WithUnused(a, b)
                ),
                Sequence(
                    _requiredOpenParen,
                    SeparatedList(
                        parameter,
                        new OperatorParser(","),
                        parameters => new ListNode<ParameterNode> { Items = parameters.ToList(), Separator = new OperatorNode(",") }
                    ),
                    _requiredCloseParen,
                    (a, parameters, b) => parameters.WithUnused(a, b)
                ),
                Error<ListNode<ParameterNode>>(false, Errors.MissingParameterList)
            ).Named("ParameterList");
        }

        private void InitializeArgumentLists()
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

            _argumentLists = First(
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

            _maybeArgumentLists = First(
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
        }

        private void InitializeStatements()
        {
            var constStmtParser = Sequence(
                // "const" <type> <ident> "=" <expression> ";"
                new KeywordParser("const"),
                _requiredType,
                _requiredIdentifier,
                _requiredEquals,
                _requiredExpression,
                _requiredSemicolon,
                (c, type, name, e, expr, s) => new ConstNode
                {
                    Location = c.Location,
                    Type = type,
                    Name = name,
                    Value = expr
                }.WithUnused(c, e, s)
            ).Named("constStmt");

            var varDeclareParser = Sequence(
                // <type> <ident> ("=" <expression>)? ";"
                _declareTypes,
                new IdentifierParser(),
                Optional(
                    Sequence(
                        new OperatorParser("="),
                        Expressions,
                        (op, expr) => expr.WithUnused(op)
                    )
                ),
                (type, name, value) => new VariableDeclareNode
                {
                    Location = type.Location,
                    Type = type,
                    Name = name,
                    Value = value is EmptyNode ? null : value
                }
            ).Named("varDeclare");
            var varDeclareStmtParser = Sequence(
                varDeclareParser,
                _requiredSemicolon,
                (v, s) => v.WithUnused(s)
            ).Named("varDeclareStmt");

            var returnStmtParser = Sequence(
                // "return" <expression>? ";"
                new KeywordParser("return"),
                Optional(Expressions),
                _requiredSemicolon,
                (r, expr, s) => new ReturnNode
                {
                    Location = r.Location,
                    Expression = expr is EmptyNode ? null : expr
                }.WithUnused(s)
            ).Named("returnStmt");

            var usingStmtParser = Sequence(
                // "using" "(" <varDeclare> | <expr> ")" <statement>
                new KeywordParser("using"),
                _requiredOpenParen,
                First(
                    varDeclareParser,
                    Expressions,
                    Error<EmptyNode>(false, Errors.MissingExpression)
                ),
                _requiredCloseParen,
                Required(Deferred(() => Statements), () => new EmptyNode(), Errors.MissingStatement),
                (u, a, disposable, b, stmt) => new UsingStatementNode
                {
                    Location = u.Location,
                    Disposable = disposable,
                    Statement = stmt
                }.WithUnused(a, b)
            ).Named("usingStmt");

            Statements = First(
                // ";" | <returnStatement> | <declaration> | <constDeclaration> | <expression>
                // <csharpLiteral> | <usingStatement>
                Transform(
                    new OperatorParser(";"),
                    o => new EmptyNode().WithUnused(o)
                ).Named("emptyStmt"),
                Token(TokenType.CSharpLiteral, x => new CSharpNode(x)),
                usingStmtParser,
                returnStmtParser,
                constStmtParser,
                varDeclareStmtParser,
                Sequence(
                    Expressions,
                    _requiredSemicolon,
                    (expr, s) => expr.WithUnused(s)
                ).Named("expressionStmt")
            ).Named("Statements");
        }

        private void InitializeTypes()
        {
            // <typeName> ("<" <typeArray> ("," <typeArray>)* ">")?
            var typeName = Transform(
                new IdentifierParser(),
                id => new TypeNode(id)
            ).Named("typeName");

            var genericType = First(
                Sequence(
                    typeName,
                    new OperatorParser("<"),
                    SeparatedList(
                        Types,
                        new OperatorParser(","),
                        list =>
                        {
                            var typeList = new ListNode<TypeNode>
                            {
                                Items = list.ToList(),
                                Separator = new OperatorNode(",")
                            };
                            if (list.Count == 0)
                                typeList.WithDiagnostics(typeList.Location, Errors.MissingType);
                            return typeList;
                        }),
                    _requiredCloseAngle,
                    (type, b, genericArgs, d) =>
                    {
                        type.GenericArguments = genericArgs;
                        return type.WithUnused(b, d);
                    }),
                typeName
            ).Named("genericType");

            var subtype = SeparatedList(
                genericType,
                new OperatorParser("."),
                t => new ListNode<TypeNode> { Items = t.ToList(), Separator = new OperatorNode(".") }
            ).Named("subtype");

            // <subtype> ("[" "]")*
            var types = Sequence(
                subtype,
                List(
                    Sequence(
                        new OperatorParser("["),
                        // TODO: Should be able to support multi-dimensional arrays here
                        _requiredCloseBrace,
                        (a, b) => new ArrayTypeNode { Location = a.Location }.WithUnused(a, b)
                    ),
                    items => new ListNode<ArrayTypeNode> { Items = items.ToList() }
                ),
                (a, b) =>
                {
                    if (a.Count == 0)
                        return null;
                    if (a.Count == 1)
                    {
                        a[0].ArrayTypes = b.Count == 0 ? null : b;
                        return a[0];
                    }

                    var current = a[a.Count - 1];
                    for (int i = a.Count - 2; i >= 0; i--)
                    {
                        a[i].Child = current;
                        current = a[i];
                    }

                    a[0].ArrayTypes = b.Count == 0 ? null : b;
                    return a[0];
                }).Named("_types");

            Types = Deferred(() => types).Named("Types");
        }

        private void InitializeDeclareTypes()
        {
            _declareTypes = First(
                Transform(
                    new KeywordParser("var"),
                    k => new TypeNode { Name = new IdentifierNode(k.Keyword), Location = k.Location }
                ),
                Transform(
                    new KeywordParser("dynamic"),
                    k => new TypeNode { Name = new IdentifierNode(k.Keyword), Location = k.Location }
                ),
                Types
            ).Named("DeclareTypes");
        }

        private void InitializeGenericTypeArguments()
        {
            // TODO: Can we remove this?
            var requiredGenericTypeArguments = Sequence(
                new OperatorParser("<"),
                SeparatedList(
                    Types,
                    new OperatorParser(","),
                    types => new ListNode<TypeNode> { Items = types.ToList(), Separator = new OperatorNode(",") },
                    atLeastOne: true
                ),
                _requiredCloseAngle,
                (a, types, b) => types.WithUnused(a, b)
            ).Named("requiredGenericTypeArguments");

            _optionalGenericTypeArguments = Optional(
                Sequence(
                    new OperatorParser("<"),
                    SeparatedList(
                        Types,
                        new OperatorParser(","),
                        types => new ListNode<TypeNode> { Items = types.ToList(), Separator = new OperatorNode(",") },
                        atLeastOne: true
                    ),
                    new OperatorParser(">"),
                    (a, types, b) => types.WithUnused(a, b)
                )
            ).Named("_optionalGenericTypeArguments");
        }

        private void InitializeGenericTypeParameters()
        {
            ListNode<IdentifierNode> ProduceGenericTypeParameterList(IReadOnlyList<IdentifierNode> types)
            {
                var listNode = new ListNode<IdentifierNode>
                {
                    Items = types.ToList(),
                    Separator = new OperatorNode(",")
                };
                if (types.Count == 0)
                    listNode.WithDiagnostics(listNode.Location, Errors.MissingType);
                return listNode;
            }

            _genericTypeParameters = First(
                Sequence(
                    new OperatorParser("<"),
                    SeparatedList(
                        new IdentifierParser(),
                        new OperatorParser(","),
                        ProduceGenericTypeParameterList),
                    _requiredCloseAngle,
                    (a, types, b) => types.WithUnused(a, b)
                ),
                Produce(() => ListNode<IdentifierNode>.Default())
            ).Named("GenericTypeParameters");
        }

        private void InitializeTypeConstraints()
        {
            var constraintList = SeparatedList(
                First<AstNode>(
                    new KeywordParser("class"),
                    Sequence(
                        new KeywordParser("new"),
                        _requiredOpenParen,
                        _requiredCloseParen,
                        (n, a, b) => new KeywordNode { Keyword = "new()", Location = n.Location }.WithUnused(a, b)
                    ).Named("newConstraint"),
                    Types
                ),
                new OperatorParser(","),
                constraints => new ListNode<AstNode> { Items = constraints.ToList(), Separator = new OperatorNode(",") }
            ).Named("constraintList");

            _typeConstraints = List(
                Sequence(
                    new KeywordParser("where"),
                    _requiredIdentifier,
                    _requiredColon,
                    constraintList,
                    (w, type, o, constraints) => new TypeConstraintNode
                    {
                        Location = w.Location,
                        Type = type,
                        Constraints = constraints
                    }.WithUnused(w, o)
                ),
                allConstraints => new ListNode<TypeConstraintNode> { Items = allConstraints.ToList() }
            ).Named("TypeConstraints");
        }

        private void InitializeNew()
        {
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
            _newParser = First(
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
                    _argumentLists,
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
        }

        private void InitializeExpressions()
        {
            // TODO: A proper precedence-based parser for expressions to try and save performance and stack space.
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
                _newParser,
                Sequence<OperatorNode, AstNode, OperatorNode, AstNode>(
                    new OperatorParser("("),
                    Deferred(() => ExpressionList),
                    _requiredCloseParen,
                    (a, expr, b) => expr.WithUnused(a, b)
                )
            ).Named("terminal");

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
                        _maybeArgumentLists,
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
                        _maybeArgumentLists,
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
                        Deferred(() => _normalMethodBody),
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

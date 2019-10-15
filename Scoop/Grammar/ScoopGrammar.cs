using System.Collections.Generic;
using System.Linq;
using Scoop.Parsers;
using Scoop.SyntaxTree;
using Scoop.Tokenization;
using static Scoop.Parsers.ScoopParsers;

namespace Scoop.Grammar
{
    public class ScoopGrammar 
    {
        public ScoopGrammar()
        {
            Initialize();
        }

        public static readonly ScoopGrammar Instance = new ScoopGrammar();

        private static readonly HashSet<string> Keywords = new HashSet<string>
        {
            // C# Keywords which are still allowed
            // (type names like "int" are counted as types not keywords for these purposes)
            // This list is mostly used to help filter out valid identifiers
            "async",
            "await",
            "class",
            "const",
            "delegate",
            "dynamic",
            "enum",
            "false",
            "interface",
            "namespace",
            "new",
            "null",
            "params",
            "partial",
            "private",
            "public",
            "return",
            "struct",
            "throw",
            "true",
            "using",
            "var",
            "where"
        };
        
        public IParser<CompilationUnitNode> CompilationUnits { get; private set; }
        public IParser<TypeNode> Types { get; private set; }
        public IParser<AstNode> Expressions { get; private set; }
        public IParser<ListNode<AstNode>> ExpressionList { get; private set; }
        public IParser<AstNode> Statements { get; private set; }
        public IParser<ListNode<AttributeNode>> Attributes { get; private set; }
        public IParser<DelegateNode> Delegates { get; private set; }
        public IParser<EnumNode> Enums { get; private set; }
        public IParser<ClassNode> Classes { get; private set; }
        public IParser<AstNode> ClassMembers { get; private set; }
        public IParser<InterfaceNode> Interfaces { get; private set; }

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
        private IParser<ClassNode> _nestedClasses;
        private IParser<ListNode<AttributeNode>> _attributeTags;

        private void Initialize()
        {
            // Setup some parsers by reference to avoid circular references and null-refs
            Types = Deferred(() => _types).Named("Types");
            Expressions = Deferred(() => _expressions).Named("Expressions");

            // Setup some commonly-used parsers
            _identifiers = new IdentifierParser(Keywords).Named("_identifiers");
            _accessModifiers = Optional(
                Keyword("public", "private")
            ).Named("accessModifiers");

            // Setup some parsers for requiring operators or communicating helpful errors
            _requiredSemicolon = Required(Operator(";"), Errors.MissingSemicolon);
            _requiredOpenBracket = Required(Operator("{"), Errors.MissingOpenBracket);
            _requiredCloseBracket = Required(Operator("}"), Errors.MissingCloseBracket);
            _requiredOpenParen = Required(Operator("("), Errors.MissingOpenParen);
            _requiredCloseParen = Required(Operator(")"), Errors.MissingCloseParen);
            _requiredColon = Required(Operator(":"), Errors.MissingColon);
            _requiredCloseBrace = Required(Operator("]"), Errors.MissingCloseBrace);
            _requiredCloseAngle = Required(Operator(">"), Errors.MissingCloseAngle);
            _requiredIdentifier = Required(_identifiers, Errors.MissingIdentifier);
            _requiredEquals = Required(Operator("="), Errors.MissingEquals);

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
                    _identifiers,
                    Operator("="),
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
                        Operator("("),
                        Operator(")"),
                        (a, b) => ListNode<AstNode>.Default()
                    ),
                    Sequence(
                        Operator("("),
                        SeparatedList(
                            argumentParser,
                            Operator(","),
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
                            // We don't support "event" or "property" targets since we don't allow those structures
                            Keyword("assembly", "module", "field", "method", "param", "return", "type"),
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
                Operator(","),
                list => new ListNode<AttributeNode> { Items = list.ToList(), Separator = new OperatorNode(",") }
            ).Named("attributeList");

            _attributeTags = Sequence(
                // "[" <attributeList> "]"
                Operator("["),
                attrParser,
                _requiredCloseBrace,
                (a, attrs, b) => attrs.WithUnused(a, b)
            ).Named("attributeTag");
            
            Attributes = Transform(
                Optional(
                    List(
                        _attributeTags,
                        list => new ListNode<AttributeNode> { Items = list.SelectMany(l => l.Items).ToList() }.WithUnused(list.SelectMany(a => a.Unused.OrEmptyIfNull()).ToArray())
                    )),
                n => n is EmptyNode ? ListNode<AttributeNode>.Default() : n as ListNode<AttributeNode>
            ).Named("Attributes");
        }

        private void InitializeTopLevel()
        {
            _dottedIdentifiers = Transform(
                SeparatedList(
                    _identifiers,
                    Operator("."),
                    items => new ListNode<IdentifierNode> { Items = items.ToList(), Separator = new OperatorNode(".") },
                    atLeastOne: true
                ),
                items => new DottedIdentifierNode(items.Select(i => i.Id), items.Location)
            ).Named("_dottedIdentifiers");

            var usingDirectives = Sequence(
                // "using" <namespaceName> ";"
                Keyword("using"),
                Required(_dottedIdentifiers, () => new DottedIdentifierNode(""), Errors.MissingNamespaceName),
                _requiredSemicolon,
                (a, b, c) => new UsingDirectiveNode
                {
                    Location = a.Location,
                    Namespace = b
                }.WithUnused(a, c)
            ).Named("usingDirectives");

            var namespaceMembers = First<AstNode>(
                Token(TokenType.CSharpLiteral, t => new CSharpNode(t)),
                Deferred(() => Classes),
                Deferred(() => Interfaces),
                Deferred(() => Enums),
                Deferred(() => Delegates)
            );
            var namespaceBody = First(
                Sequence(
                    Operator("{"),
                    Operator("}"),
                    (a, b) => new ListNode<AstNode>()
                ),
                Sequence(
                    Operator("{"),
                    List(
                        namespaceMembers,
                        members => new ListNode<AstNode> { Items = members.ToList() }
                    ),
                    _requiredCloseBracket,
                    (a, members, b) => members.WithUnused(a, b)
                ),
                Error<ListNode<AstNode>>(false, Errors.MissingOpenBracket)
            );
            var namespaces = Sequence(
                Keyword("namespace"),
                Required(_dottedIdentifiers, () => new DottedIdentifierNode(""), Errors.MissingNamespaceName),
                namespaceBody,
                (ns, name, members) => new NamespaceNode
                {
                    Location = ns.Location,
                    Name = name,
                    Declarations = members
                }.WithUnused(ns)
            ).Named("namespaces");

            CompilationUnits = Transform(
                List(
                    First<AstNode>(
                        usingDirectives,
                        namespaces,
                        Deferred(() => _attributeTags)
                    ),
                    items => new ListNode<AstNode> { Items = items.ToList() }
                ),
                list => new CompilationUnitNode
                {
                    Members = list
                }
            ).Named("CompilationUnits");
        }

        private void InitializeEnums()
        {
            var enumMember = Sequence(
                Attributes,
                _identifiers,
                Optional(
                    Sequence(
                        Operator("="),
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
                Keyword("enum"),
                _requiredIdentifier,
                _requiredOpenBracket,
                SeparatedList(
                    enumMember,
                    Operator(","),
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
                Keyword("delegate"),
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
                    Operator(":"),
                    Required(
                        SeparatedList(
                            Types,
                            Operator(","),
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
                    Operator("{"),
                    Operator("}"),
                    (a, b) => new ListNode<MethodDeclareNode>().WithUnused(a, b)
                ),
                Sequence(
                    Operator("{"),
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
                Keyword("interface"),
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
                Keyword("const"),
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
                Operator("=>"),
                First(
                    Sequence(
                        Operator("{"),
                        Operator("}"),
                        (a, b) => new ListNode<AstNode>().WithUnused(a, b)
                    ),
                    Sequence(
                        Operator("{"),
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
                    Operator("{"),
                    Operator("}"),
                    (a, b) => new ListNode<AstNode>().WithUnused(a, b)
                ),
                Sequence(
                    Operator("{"),
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

            var constructors = First(
                Replaceable(Fail<ConstructorNode>()).Named("constructorNamedStub"),
                Sequence(
                    Attributes,
                    _accessModifiers,
                    _identifiers,
                    _parameterLists,
                    Optional(
                        Sequence(
                            Operator(":"),
                            Required(Keyword("this"), Errors.MissingThis),
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
                ).Named("constructorNormal")
            ).Named("constructors");

            var methods = Sequence(
                // <accessModifier>? "async"? <type> <ident> <genericTypeParameters>? <parameterList> <typeConstraints>? <methodBody>
                Attributes,
                _accessModifiers,
                // TODO: If we see "async" it must be a method and we should require everything else. No backtracking after that
                Optional(Keyword("async")),
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
                Deferred(() => _nestedClasses),
                Interfaces,
                Enums,
                Delegates,
                constants,
                fields,
                methods,
                Replaceable(Fail<MethodNode>()).Named("Method1"),
                Replaceable(Fail<MethodNode>()).Named("Method2"),
                constructors
            ).Named("ClassMembers");

            var classBody = First(
                Sequence(
                    Operator("{"),
                    Operator("}"),
                    (a, b) => new ListNode<AstNode>().WithUnused(a, b)
                ).Named("classBody.EmptyBrackets"),
                Sequence(
                    Operator("{"),
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
                Optional(Keyword("public")),
                Optional(Keyword("partial")),
                Keyword("class", "struct"),
                _requiredIdentifier,
                _genericTypeParameters,
                inheritanceList,
                _typeConstraints,
                classBody,
                (attrs, vis, isPartial, obj, name, genParm, contracts, cons, body) => new ClassNode
                {
                    Attributes = attrs.IsNullOrEmpty() ? null : attrs,
                    AccessModifier = (vis as KeywordNode) ?? new KeywordNode("public"),
                    Modifiers = isPartial is KeywordNode k ? new ListNode<KeywordNode> { k } : null,
                    Type = obj,
                    Name = name,
                    GenericTypeParameters = genParm.IsNullOrEmpty() ? null : genParm,
                    Interfaces = contracts as ListNode<TypeNode>,
                    TypeConstraints = cons.IsNullOrEmpty() ? null : cons,
                    Members = body
                }
            ).Named("Classes");

            _nestedClasses = Sequence(
                Attributes,
                _accessModifiers,
                Optional(Keyword("partial")),
                Keyword("class", "struct"),
                _requiredIdentifier,
                _genericTypeParameters,
                inheritanceList,
                _typeConstraints,
                classBody,
                (attrs, vis, isPartial, obj, name, genParm, contracts, cons, body) => new ClassNode
                {
                    Attributes = attrs.IsNullOrEmpty() ? null : attrs,
                    AccessModifier = (vis as KeywordNode) ?? new KeywordNode("private"),
                    Modifiers = isPartial is KeywordNode k ? new ListNode<KeywordNode> { k } : null,
                    Type = obj,
                    Name = name,
                    GenericTypeParameters = genParm.IsNullOrEmpty() ? null : genParm,
                    Interfaces = contracts as ListNode<TypeNode>,
                    TypeConstraints = cons.IsNullOrEmpty() ? null : cons,
                    Members = body
                }
            ).Named("_nestedClasses");
        }

        private void InitializeParameters()
        {
            var parameter = Sequence(
                // <attributes> "params"? <type> <ident> ("=" <expr>)?
                Attributes,
                Optional(Keyword("params")),
                Types,
                _requiredIdentifier,
                Optional(
                    Sequence(
                        Operator("="),
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
                    Operator("("),
                    Operator(")"),
                    (a, b) => ListNode<ParameterNode>.Default().WithUnused(a, b)
                ),
                Sequence(
                    _requiredOpenParen,
                    SeparatedList(
                        parameter,
                        Operator(","),
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
                    _identifiers,
                    Operator(":"),
                    _requiredExpression,
                    (name, s, expr) => new NamedArgumentNode { Name = name, Separator = s, Value = expr }
                ),
                Expressions
            ).Named("arguments");

            _argumentLists = First(
                // A required argument list
                // "(" <commaSeparatedArgs>? ")"
                Sequence(
                    Operator("("),
                    Operator(")"),
                    (a, b) => ListNode<AstNode>.Default()
                ),
                Sequence(
                    _requiredOpenParen,
                    SeparatedList(
                        arguments,
                        Operator(","),
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
                    Operator("("),
                    Operator(")"),
                    (a, b) => ListNode<AstNode>.Default()
                ),
                Sequence(
                    Operator("("),
                    SeparatedList(
                        arguments,
                        Operator(","),
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
                Keyword("const"),
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
                _identifiers,
                Optional(
                    Sequence(
                        Operator("="),
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
                Keyword("return"),
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
                Keyword("using"),
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
                    Operator(";"),
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

        private IParser<TypeNode> _types;

        private void InitializeTypes()
        {
            // <typeName> ("<" <typeArray> ("," <typeArray>)* ">")?
            var typeName = Transform(
                _identifiers,
                id => new TypeNode(id)
            ).Named("typeName");

            var genericType = First(
                Sequence(
                    typeName,
                    Operator("<"),
                    SeparatedList(
                        Types,
                        Operator(","),
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
                Operator("."),
                t => new ListNode<TypeNode> { Items = t.ToList(), Separator = new OperatorNode(".") }
            ).Named("subtype");

            // <subtype> ("[" "]")*
            _types = Sequence(
                subtype,
                List(
                    Sequence(
                        Operator("["),
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
        }

        private void InitializeDeclareTypes()
        {
            _declareTypes = First(
                Transform(
                    Keyword("var"),
                    k => new TypeNode { Name = new IdentifierNode(k.Keyword), Location = k.Location }
                ),
                Transform(
                    Keyword("dynamic"),
                    k => new TypeNode { Name = new IdentifierNode(k.Keyword), Location = k.Location }
                ),
                Types
            ).Named("DeclareTypes");
        }

        private void InitializeGenericTypeArguments()
        {
            _optionalGenericTypeArguments = Optional(
                Sequence(
                    Operator("<"),
                    SeparatedList(
                        Types,
                        Operator(","),
                        types => new ListNode<TypeNode> { Items = types.ToList(), Separator = new OperatorNode(",") },
                        atLeastOne: true
                    ),
                    Operator(">"),
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
                    Operator("<"),
                    SeparatedList(
                        _identifiers,
                        Operator(","),
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
                    Keyword("class"),
                    Sequence(
                        Keyword("new"),
                        _requiredOpenParen,
                        _requiredCloseParen,
                        (n, a, b) => new KeywordNode { Keyword = "new()", Location = n.Location }.WithUnused(a, b)
                    ).Named("newConstraint"),
                    Types
                ),
                Operator(","),
                constraints => new ListNode<AstNode> { Items = constraints.ToList(), Separator = new OperatorNode(",") }
            ).Named("constraintList");

            _typeConstraints = List(
                Sequence(
                    Keyword("where"),
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
                    Operator("="),
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
                    Operator("["),
                    SeparatedList(
                        Expressions,
                        Operator(","),
                        items => new ListNode<AstNode>
                        {
                            Items = items.ToList(),
                            Separator = new OperatorNode(",")
                        }
                    ),
                    Operator("]"),
                    Operator("="),
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
                    Operator("{"),
                    SeparatedList(
                        Expressions,
                        Operator(","),
                        items => new ListNode<AstNode>
                        {
                            Items = items.ToList(),
                            Separator = new OperatorNode(",")
                        }
                    ),
                    Operator("}"),
                    (o, args, c) => new AddInitializerNode
                    {
                        Location = o.Location,
                        Arguments = args
                    }.WithUnused(o, c)
                )
            );
            var initializers = Sequence(
                Operator("{"),
                First(
                    SeparatedList(
                        nonCollectionInitializer,
                        Operator(","),
                        items => new ListNode<AstNode>
                        {
                            Items = items.ToList(),
                            Separator = new OperatorNode(",")
                        },
                        atLeastOne: true
                    ),
                    SeparatedList(
                        Expressions,
                        Operator(","),
                        items => new ListNode<AstNode>
                        {
                            Items = items.ToList(),
                            Separator = new OperatorNode(",")
                        }
                    ).Named("initializers.Collection"),
                    Produce(() => ListNode<AstNode>.Default()).Named("initializers.Empty")
                ),
                Operator("}"),
                (a, inits, b) => inits.WithUnused(a, b)
            ).Named("initializers");

            _newParser = First(
                // "new" "{" <initializers> "}"
                Sequence(
                    Keyword("new"),
                    initializers,
                    (n, inits) => new NewNode
                    {
                        Location = n.Location,
                        Initializers = inits
                    }
                ),
                // "new" <type> "{" <initializers> "}"
                Replaceable(
                    Sequence(
                        Keyword("new"),
                        Types,
                        initializers,
                        (n, type, inits) => new NewNode
                        {
                            Location = n.Location,
                            Type = type,
                            Initializers = inits
                        }
                    )
                ).Named("newInits"),
                Replaceable(Fail<NewNode>()).Named("newNamedArgsInitsStub"),
                // "new" <type> <arguments> <initializers>?
                Replaceable(
                    Sequence(
                        Keyword("new"),
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
                ).Named("newArgsInits")
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
                Keyword("true", "false", "null", "this"),
                _newParser,
                _identifiers,
                Token(TokenType.String, x => new StringNode(x)),
                Token(TokenType.Character, x => new CharNode(x)),
                Token(TokenType.Integer, x => new IntegerNode(x)),
                Token(TokenType.UInteger, x => new UIntegerNode(x)),
                Token(TokenType.Long, x => new LongNode(x)),
                Token(TokenType.ULong, x => new ULongNode(x)),
                Token(TokenType.Decimal, x => new DecimalNode(x)),
                Token(TokenType.Float, x => new FloatNode(x)),
                Token(TokenType.Double, x => new DoubleNode(x)),
                Sequence<OperatorNode, AstNode, OperatorNode, AstNode>(
                    Operator("("),
                    Deferred(() => ExpressionList),
                    _requiredCloseParen,
                    (a, expr, b) => expr.WithUnused(a, b)
                )
            ).Named("terminal");

            var indexers = Sequence(
                Operator("["),
                SeparatedList(
                    Expressions,
                    Operator(","),
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
                        Operator("++", "--"),
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
                        Operator(".", "?."),
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
                        Operator(".", "?."),
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
                // prefix ++ and -- cannot be combined with other prefix operators
                Sequence(
                    Operator("++", "--"),
                    expressionPostfix,
                    (op, expr) => new PrefixOperationNode
                    {
                        Location = op.Location,
                        Operator = op,
                        Right = expr
                    }
                ),
                // ("-" | "+" | "~" | "!" | "await" | "throw" | <cast>)* <postfix>
                Sequence(
                    List(
                        First<AstNode>(
                            Operator("-", "+", "!", "~"),
                            Transform(
                                Keyword("await", "throw"),
                                k => new OperatorNode(k.Keyword, k.Location)
                            ),
                            Sequence(
                                Operator("("),
                                Types,
                                Operator(")"),
                                (a, type, b) => type
                            )
                        ),
                        ops => new ListNode<AstNode> { Items = ops.ToList() }
                    ),
                    expressionPostfix,
                    (ops, expr) =>
                    {
                        var current = expr;
                        for (int i = ops.Items.Count - 1; i >= 0; i--)
                        {
                            var prefix = ops[i];
                            if (prefix is OperatorNode op)
                            {
                                current = new PrefixOperationNode
                                {
                                    Location = op.Location,
                                    Operator = op,
                                    Right = current
                                };
                            }
                            else if (prefix is TypeNode type)
                            {
                                current = new CastNode
                                {
                                    Location = type.Location,
                                    Type = type,
                                    Right = current
                                };
                            }
                        }

                        return current;
                    }
                ),
                // <postfix>
                expressionPostfix
            ).Named("unary");

            var expressionMultiplicative = Infix(
                // Operators with * / % precidence
                // <Unary> (<op> <Unary>)+
                expressionUnary,
                Operator("*", "/", "%"),
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
                Operator("+", "-"),
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
                        Operator("as", "is"),
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
                Operator("==", "!=", ">=", "<=", "<", ">"),
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
                Operator("&", "^", "|"),
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
                Operator("&&", "||"),
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
                Operator("??"),
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
                        Operator("?"),
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
                Operator("=", "+=", "-=", "/=", "%="),
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
                            _identifiers,
                            id => new ListNode<IdentifierNode> { Separator = new OperatorNode(","), [0] = id }
                        ),
                        Sequence(
                            Operator("("),
                            Operator(")"),
                            (a, b) => new ListNode<IdentifierNode> { Separator = new OperatorNode(","), Items = new List<IdentifierNode>() }
                        ),
                        Sequence(
                            Operator("("),
                            SeparatedList(
                                _identifiers,
                                Operator(","),
                                args => new ListNode<IdentifierNode> { Items = args.ToList(), Separator = new OperatorNode(",") }
                            ),
                            Operator(")"),
                            (a, items, c) => new ListNode<IdentifierNode> { Separator = new OperatorNode(","), Items = items.ToList() }
                        )
                    ),
                    Operator("=>"),
                    First(
                        Deferred(() => _normalMethodBody),
                        Transform(
                            expressionAssignment,
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
                Operator(","),
                items => new ListNode<AstNode> { Items = items.ToList(), Separator = new OperatorNode(",") },
                atLeastOne: true
            ).Named("ExpressionList");
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using Scoop.Parsers;
using Scoop.SyntaxTree;
using Scoop.Tokenization;
using static Scoop.Parsers.ParserMethods;
using static Scoop.Parsers.TokenParserMethods;

namespace Scoop.Grammar
{
    public class ScoopGrammar 
    {
        public ScoopGrammar()
        {
            Initialize();
        }

        private static readonly HashSet<string> _keywords = new HashSet<string>
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
        
        public IParser<Token, CompilationUnitNode> CompilationUnits { get; private set; }
        public IParser<Token, TypeNode> Types { get; private set; }
        public IParser<Token, AstNode> Expressions { get; private set; }
        public IParser<Token, ListNode<AstNode>> ExpressionList { get; private set; }
        public IParser<Token, AstNode> Statements { get; private set; }
        public IParser<Token, ListNode<AttributeNode>> Attributes { get; private set; }
        public IParser<Token, DelegateNode> Delegates { get; private set; }
        public IParser<Token, EnumNode> Enums { get; private set; }
        public IParser<Token, ClassNode> Classes { get; private set; }
        public IParser<Token, AstNode> ClassMembers { get; private set; }
        public IParser<Token, InterfaceNode> Interfaces { get; private set; }

        private IParser<Token, KeywordNode> _accessModifiers;
        private IParser<Token, IdentifierNode> _identifiers;
        private IParser<Token, OperatorNode> _requiredSemicolon;
        private IParser<Token, OperatorNode> _requiredOpenBracket;
        private IParser<Token, OperatorNode> _requiredCloseBracket;
        private IParser<Token, OperatorNode> _requiredOpenParen;
        private IParser<Token, OperatorNode> _requiredCloseParen;
        private IParser<Token, OperatorNode> _requiredColon;
        private IParser<Token, OperatorNode> _requiredCloseBrace;
        private IParser<Token, OperatorNode> _requiredCloseAngle;
        private IParser<Token, OperatorNode> _requiredEquals;
        private IParser<Token, IdentifierNode> _requiredIdentifier;
        private IParser<Token, TypeNode> _requiredType;
        private IParser<Token, TypeNode> _declareTypes;
        private IParser<Token, ListNode<IdentifierNode>> _genericTypeParameters;
        private IParser<Token, ListNode<AstNode>> _argumentLists;
        private IParser<Token, ListNode<AstNode>> _maybeArgumentLists;
        private IParser<Token, AstNode> _requiredExpression;
        private IParser<Token, ListNode<TypeNode>> _optionalGenericTypeArguments;
        private IParser<Token, ListNode<ParameterNode>> _parameterLists;
        private IParser<Token, ListNode<TypeConstraintNode>> _typeConstraints;
        private IParser<Token, ListNode<AstNode>> _normalMethodBody;
        private IParser<Token, NewNode> _newParser;
        private IParser<Token, DottedIdentifierNode> _dottedIdentifiers;
        private IParser<Token, AstNode> _expressions;
        private IParser<Token, AstNode> _expressionConditional;
        private IParser<Token, ClassNode> _nestedClasses;
        private IParser<Token, ListNode<AttributeNode>> _attributeTags;
        private IParser<Token, TypeNode> _types;

        private void Initialize()
        {
            // Setup some parsers by reference to avoid circular references and null-refs
            Types = Deferred(() => _types).Named("Types");
            Expressions = Deferred(() => _expressions).Named("Expressions");

            // Setup some commonly-used parsers
            _identifiers = new PredicateParser<Token, IdentifierNode>(
                t => t.IsType(TokenType.Word) && !_keywords.Contains(t.Value), 
                t => new IdentifierNode(t)).Named("_identifiers");
            _accessModifiers = Optional(
                Keyword("public", "private")
            ).Named("accessModifiers");

            // Setup some parsers for requiring operators or communicating helpful errors
            _requiredSemicolon = Required(Operator(";"), t => new OperatorNode().WithDiagnostics(t.CurrentLocation, Errors.MissingSemicolon));
            _requiredOpenBracket = Required(Operator("{"), t => new OperatorNode().WithDiagnostics(t.CurrentLocation, Errors.MissingOpenBracket));
            _requiredCloseBracket = Required(Operator("}"), t => new OperatorNode().WithDiagnostics(t.CurrentLocation, Errors.MissingCloseBracket));
            _requiredOpenParen = Required(Operator("("), t => new OperatorNode().WithDiagnostics(t.CurrentLocation, Errors.MissingOpenParen));
            _requiredCloseParen = Required(Operator(")"), t => new OperatorNode().WithDiagnostics(t.CurrentLocation, Errors.MissingCloseParen));
            _requiredColon = Required(Operator(":"), t => new OperatorNode().WithDiagnostics(t.CurrentLocation, Errors.MissingColon));
            _requiredCloseBrace = Required(Operator("]"), t => new OperatorNode().WithDiagnostics(t.CurrentLocation, Errors.MissingCloseBrace));
            _requiredCloseAngle = Required(Operator(">"), t => new OperatorNode().WithDiagnostics(t.CurrentLocation, Errors.MissingCloseAngle));
            _requiredIdentifier = Required(_identifiers, t => new IdentifierNode().WithDiagnostics(t.CurrentLocation, Errors.MissingIdentifier));
            _requiredEquals = Required(Operator("="), t => new OperatorNode().WithDiagnostics(t.CurrentLocation, Errors.MissingEquals));

            // Parsers to require certain productions or add a helpful error
            _requiredType = Required(Types, t => new TypeNode().WithDiagnostics(t.CurrentLocation, Errors.MissingType));
            _requiredExpression = Required(Expressions, t => new EmptyNode().WithDiagnostics(t.CurrentLocation, Errors.MissingExpression));

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
            var attributeArgs = First(
                // (<identifier> "=" <expr>) | <expr>
                Sequence(
                    _identifiers,
                    Operator("="),
                    // TODO: I think these expressions can only be terminals or member accesses (consts or enums, etc)
                    Expressions,
                    (name, s, expr) => (AstNode)new NamedArgumentNode { Name = name, Separator = s, Value = expr }
                ),
                // TODO: I think these expressions can only be terminals or member accesses (consts or enums, etc)
                Expressions
            ).Named("attributeArgument");

            var argumentListParser = Optional(
                // ("(" <argumentList> ")"))?
                Sequence(
                    Operator("("),
                    SeparatedList(
                        attributeArgs,
                        Operator(","),
                        items => new ListNode<AstNode> { Items = items.ToList(), Separator = new OperatorNode(",") }
                    ),
                    _requiredCloseParen,
                    (a, items, c) => items.WithUnused(a, c)
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
                        Target = target,
                        Type = type,
                        Arguments = args
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
            
            Attributes = Optional(
                List(
                    _attributeTags,
                    list => new ListNode<AttributeNode> { Items = list.SelectMany(l => l.Items).ToList() }.WithUnused(list.SelectMany(a => a.Unused.OrEmptyIfNull()).ToArray())
                )
            ).Named("Attributes");
        }

        private void InitializeTopLevel()
        {
            _dottedIdentifiers = SeparatedList(
                Token(TokenType.Word, t => t.Value),
                Operator("."),
                items => new DottedIdentifierNode(items),
                atLeastOne: true
            ).Named("_dottedIdentifiers");

            var usingDirectives = Sequence(
                // "using" <namespaceName> ";"
                Keyword("using"),
                Required(_dottedIdentifiers, t => new DottedIdentifierNode("").WithDiagnostics(t.CurrentLocation, Errors.MissingNamespaceName)),
                _requiredSemicolon,
                (a, b, c) => new UsingDirectiveNode
                {
                    Location = a.Location,
                    Namespace = b
                }.WithUnused(a, c)
            ).Named("usingDirectives");

            var namespaceMembers = First<Token, AstNode>(
                Token(TokenType.CSharpLiteral, t => new CSharpNode(t)),
                Deferred(() => Classes),
                Deferred(() => Interfaces),
                Deferred(() => Enums),
                Deferred(() => Delegates)
            );
            var namespaceBody = Sequence(
                Operator("{"),
                List(
                    namespaceMembers,
                    members => new ListNode<AstNode> { Items = members.ToList() }
                ),
                _requiredCloseBracket,
                (a, members, b) => members.WithUnused(a, b)
            );
            var namespaces = Sequence(
                Keyword("namespace"),
                Required(_dottedIdentifiers, t => new DottedIdentifierNode("").WithDiagnostics(t.CurrentLocation, Errors.MissingNamespaceName)),
                Required(namespaceBody, t=> new ListNode<AstNode>().WithDiagnostics(t.CurrentLocation, Errors.MissingOpenBracket)),
                (ns, name, members) => new NamespaceNode
                {
                    Location = ns.Location,
                    Name = name,
                    Declarations = members
                }.WithUnused(ns)
            ).Named("namespaces");

            CompilationUnits = List(
                First<Token, AstNode>(
                    usingDirectives, 
                    namespaces,
                    Deferred(() => _attributeTags)
                ),
                items => new CompilationUnitNode
                {
                    Members = new ListNode<AstNode> { Items = items.ToList() }
                }
            ).Named("CompilationUnits");
            // TODO: Should we have a reule expecting explicit EndOfInput?
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
                    Value = value
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
                    AccessModifier = vis,
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
                    AccessModifier = vis,
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
                        t => new ListNode<TypeNode>().WithDiagnostics(t.CurrentLocation, Errors.MissingType)
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

            var interfaceBody = Required(
                Sequence(
                    Operator("{"),
                    List(
                        interfaceMember,
                        members => new ListNode<MethodDeclareNode> { Items = members.ToList() }
                    ),
                    _requiredCloseBracket,
                    (a, members, b) => members.WithUnused(a, b)
                ),
                t => new ListNode<MethodDeclareNode>().WithDiagnostics(t.CurrentLocation, Errors.MissingOpenBracket)
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
                    AccessModifier = vis,
                    Name = name,
                    GenericTypeParameters = genParm.IsNullOrEmpty() ? null : genParm,
                    Interfaces = inh,
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
                    AccessModifier = vis,
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
                    Error<ListNode<AstNode>>(Errors.MissingOpenBracket)
                ),
                (lambda, body) => body.WithUnused(lambda)
            ).Named("exprMethodBody");

            _normalMethodBody = Sequence(
                Operator("{"),
                List(
                    Statements,
                    stmts => new ListNode<AstNode> { Items = stmts.ToList() }
                ),
                _requiredCloseBracket,
                (a, body, b) => body.WithUnused(a, b)
            ).Named("NormalMethodBody");

            var methodBody = First(
                exprMethodBody,
                _normalMethodBody,
                Error<ListNode<AstNode>>(Errors.MissingOpenBracket)
            ).Named("methodBody");

            var constructors = First(
                Replaceable<Token, ConstructorNode>().Named("constructorNamedStub"),
                Sequence(
                    Attributes,
                    _accessModifiers,
                    _identifiers,
                    _parameterLists,
                    Optional(
                        Sequence(
                            Operator(":"),
                            Required(Keyword("this"), t => new KeywordNode().WithDiagnostics(t.CurrentLocation, Errors.MissingThis)),
                            _argumentLists,
                            (a, b, args) => args.WithUnused(a, b)
                        )
                    ).Named("thisArgs"),
                    methodBody,
                    (attrs, vis, name, param, targs, body) => new ConstructorNode
                    {
                        Attributes = attrs.IsNullOrEmpty() ? null : attrs,
                        Location = name.Location,
                        AccessModifier = vis,
                        ClassName = name,
                        Parameters = param,
                        ThisArgs = targs,
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
                    AccessModifier = vis,
                    Modifiers = isAsync == null ? null : new ListNode<KeywordNode> { isAsync },
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

            ClassMembers = First(
                Token(TokenType.CSharpLiteral, cs => new CSharpNode(cs)),
                Deferred(() => _nestedClasses),
                Interfaces,
                Enums,
                Delegates,
                constants,
                fields,
                methods,
                Replaceable<Token, AstNode>().Named("Method1"),
                Replaceable<Token, AstNode>().Named("Method2"),
                constructors
            ).Named("ClassMembers");

            var classBody = Sequence(
                Operator("{"),
                List(
                    ClassMembers,
                    members => new ListNode<AstNode> { Items = members.ToList() }
                ),
                _requiredCloseBracket,
                (a, members, b) => members.WithUnused(a, b)
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
                Required(classBody, t => new ListNode<AstNode>().WithDiagnostics(t.CurrentLocation, Errors.MissingOpenBracket)),
                (attrs, vis, isPartial, obj, name, genParm, contracts, cons, body) => new ClassNode
                {
                    Attributes = attrs.IsNullOrEmpty() ? null : attrs,
                    AccessModifier = vis ?? new KeywordNode("public"),
                    Modifiers = isPartial is KeywordNode k ? new ListNode<KeywordNode> { k } : null,
                    Type = obj,
                    Name = name,
                    GenericTypeParameters = genParm.IsNullOrEmpty() ? null : genParm,
                    Interfaces = contracts,
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
                Required(classBody, t => new ListNode<AstNode>().WithDiagnostics(t.CurrentLocation, Errors.MissingOpenBracket)),
                (attrs, vis, isPartial, obj, name, genParm, contracts, cons, body) => new ClassNode
                {
                    Attributes = attrs.IsNullOrEmpty() ? null : attrs,
                    AccessModifier = vis ?? new KeywordNode("private"),
                    Modifiers = isPartial is KeywordNode k ? new ListNode<KeywordNode> { k } : null,
                    Type = obj,
                    Name = name,
                    GenericTypeParameters = genParm.IsNullOrEmpty() ? null : genParm,
                    Interfaces = contracts,
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
                    IsParams = isparams != null,
                    Type = type,
                    Name = name,
                    DefaultValue = value
                }
            ).Named("parameter");

            _parameterLists = Required(
                // ("(" <commaSeparatedParameterList> ")")
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
                t => new ListNode<ParameterNode>().WithDiagnostics(t.CurrentLocation, Errors.MissingParameterList)
            ).Named("ParameterList");
        }

        private void InitializeArgumentLists()
        {
            var arguments = First(
                Sequence(
                    _identifiers,
                    Operator(":"),
                    _requiredExpression,
                    (name, s, expr) => (AstNode)new NamedArgumentNode { Name = name, Separator = s, Value = expr }
                ),
                Expressions
            ).Named("arguments");

            _argumentLists = Sequence(
                // A required argument list
                // "(" <commaSeparatedArgs>? ")"
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
            ).Named("ArgumentLists");

            _maybeArgumentLists = Sequence(
                // An optional argument list, is able to fail without diagnostics
                // "(" <commaSeparatedArgs>? ")"
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
                    Value = value
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
                    Expression = expr
                }.WithUnused(s)
            ).Named("returnStmt");

            var usingStmtParser = Sequence(
                // "using" "(" <varDeclare> | <expr> ")" <statement>
                Keyword("using"),
                _requiredOpenParen,
                First(
                    varDeclareParser,
                    Expressions,
                    Error<EmptyNode>(Errors.MissingExpression)
                ),
                _requiredCloseParen,
                Required(Deferred(() => Statements), t => new EmptyNode().WithDiagnostics(t.CurrentLocation, Errors.MissingStatement)),
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
                Transform(Operator(";"), o => new EmptyNode().WithUnused(o)).Named("emptyStmt"),
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
            // <ypeName = <identifier>
            var typeName = Transform(
                _identifiers,
                id => new TypeNode(id)
            ).Named("typeName");

            // generictype = <typeName> ("<" <Type> ("," <Type>)* ">")?
            var genericType = Sequence(
                typeName,
                Optional(
                    Sequence(
                        Operator("<"),
                        Required(
                            SeparatedList(
                                Types,
                                Operator(","),
                                list => new ListNode<TypeNode>
                                {
                                    Items = list.ToList(),
                                    Separator = new OperatorNode(",")
                                },
                                atLeastOne: true
                            ), 
                            t => new ListNode<TypeNode>().WithDiagnostics(t.CurrentLocation, Errors.MissingType)
                        ),
                        _requiredCloseAngle,
                        (b, genericArgs, d) => genericArgs.WithUnused(b, d)
                    )
                ),
                (type, genericArgs) =>
                {
                    type.GenericArguments = genericArgs;
                    return type;
                }
            ).Named("genericType");

            // subtype = <genericType> ("." <genericType>)*
            var subtype = SeparatedList(
                genericType,
                Operator("."),
                t => new ListNode<TypeNode> { Items = t.ToList(), Separator = new OperatorNode(".") },
                atLeastOne: true
            ).Named("subtype");

            // types = <subtype> ("[" ","* "]")*
            _types = Sequence(
                subtype,
                List(
                    Sequence(
                        Operator("["),
                        List(Operator(","), c => c),
                        _requiredCloseBrace,
                        (open, commas, close) => new ArrayTypeNode 
                        { 
                            Location = open.Location,
                            Dimensions = commas.Count + 1
                        }.WithUnused(open, close)
                    ),
                    items => new ListNode<ArrayTypeNode> { Items = items.ToList() }
                ),
                (subtypes, arraySpecs) =>
                {
                    if (subtypes.Count == 0)
                        return null;
                    if (subtypes.Count == 1)
                    {
                        subtypes[0].ArrayTypes = arraySpecs.Count == 0 ? null : arraySpecs;
                        return subtypes[0];
                    }

                    var current = subtypes[subtypes.Count - 1];
                    for (int i = subtypes.Count - 2; i >= 0; i--)
                    {
                        subtypes[i].Child = current;
                        current = subtypes[i];
                    }

                    subtypes[0].ArrayTypes = arraySpecs.Count == 0 ? null : arraySpecs;
                    return subtypes[0];
                }).Named("_types");
        }

        private void InitializeDeclareTypes()
        {
            _declareTypes = First(
                Transform(
                    Keyword("var", "dynamic"),
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
            _genericTypeParameters = First(
                Sequence(
                    Operator("<"),
                    SeparatedList(
                        _identifiers,
                        Operator(","),
                        types => new ListNode<IdentifierNode>
                        {
                            Items = types.ToList(),
                            Separator = new OperatorNode(",")
                        },
                        atLeastOne: true
                    ),
                    _requiredCloseAngle,
                    (a, types, b) => types.WithUnused(a, b)
                ),
                Produce<Token, ListNode<IdentifierNode>>(() => ListNode<IdentifierNode>.Default())
            ).Named("GenericTypeParameters");
        }

        private void InitializeTypeConstraints()
        {
            var newConstraint = Sequence(
                Keyword("new"),
                _requiredOpenParen,
                _requiredCloseParen,
                (n, a, b) => (AstNode) new KeywordNode { Keyword = "new()", Location = n.Location }.WithUnused(a, b)
            ).Named("newConstraint");

            var constraintList = First(
                Transform(newConstraint, n => new ListNode<AstNode> { Items = new List<AstNode> { n }, Separator = new OperatorNode(",") }),
                Sequence(
                    First<Token, AstNode>(
                        Keyword("class"),
                        Types
                    ),
                    List(
                        Sequence(
                            Operator(","),
                            Types,
                            (comma, constraint) => constraint
                        ),
                        c => c
                    ),
                    Optional(
                        Sequence(
                            Operator(","),
                            newConstraint,
                            (comma, n) => n.WithUnused(comma)
                        )
                    ),
                    (first, most, last) =>
                    {
                        var list = new List<AstNode> { first };
                        list.AddRange(most);
                        if (last != null)
                            list.Add(last);
                        return new ListNode<AstNode> { Items = list, Separator = new OperatorNode(",") };
                    }
                ),
                Error<ListNode<AstNode>>(Errors.MissingExpression)
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
            var nonCollectionInitializer = First(
                Sequence(
                    // <ident> "=" <Expression>
                    _identifiers,
                    Operator("="),
                    _requiredExpression, 
                    (name, e, expr) => (AstNode)new PropertyInitializerNode
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
                    _requiredCloseBrace,
                    _requiredEquals,
                    _requiredExpression,
                    (o, args, c, e, value) => (AstNode)new IndexerInitializerNode
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
                        },
                        atLeastOne: true
                    ),
                    _requiredCloseBracket,
                    (o, args, c) => (AstNode)new AddInitializerNode
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
                    Produce<Token, ListNode<AstNode>>(() => ListNode<AstNode>.Default()).Named("initializers.Empty")
                ),
                _requiredCloseBracket,
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
                Replaceable<Token, NewNode>().Named("newNamedArgsInitsStub"),
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
                            Initializers = inits
                        }
                    )
                ).Named("newArgsInits")
            ).Named("new");
        }

        private void InitializeExpressions()
        {
            // TODO: A proper precedence-based parser for expressions to try and save performance and stack space.
            var terminal = First<Token, AstNode>(
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
                Sequence(
                    Operator("("),
                    Deferred(() => ExpressionList),
                    _requiredCloseParen,
                    (a, expr, b) => expr.WithUnused(a, b)
                )
            ).Named("terminal");

            var indexers = Sequence(
                Operator("["),
                Required(
                    SeparatedList(
                        Expressions,
                        Operator(","),
                        items => new ListNode<AstNode>
                        {
                            Items = items.ToList(),
                            Separator = new OperatorNode(",")
                        },
                        atLeastOne: true
                    ),
                    t => new ListNode<AstNode>().WithDiagnostics(t.CurrentLocation, Errors.MissingExpression)
                ),
                _requiredCloseBrace,
                (a, items, b) => items.WithUnused(a, b)
            ).Named("indexers");

            var expressionPostfix = ApplyPostfix(
                terminal,
                init => First<Token, AstNode>(
                    // TODO: I think there's a problem where we could do something like <terminal>++() which isn't allowed
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
                                GenericArguments = genArgs
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
                    (op, expr) => (AstNode)new PrefixOperationNode
                    {
                        Location = op.Location,
                        Operator = op,
                        Right = expr
                    }
                ),
                // ("-" | "+" | "~" | "!" | "await" | "throw" | <cast>)* <postfix>
                Sequence(
                    List(
                        First<Token, AstNode>(
                            Operator("-", "+", "!", "~"),
                            Transform(Keyword("await", "throw"), n => new OperatorNode(n.Keyword, n.Location)),
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
                Required(expressionUnary, t => new EmptyNode().WithDiagnostics(t.CurrentLocation, Errors.MissingExpression)),
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
                Required(expressionMultiplicative, t => new EmptyNode().WithDiagnostics(t.CurrentLocation, Errors.MissingExpression)),
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
                        (left, op, type, name) => (AstNode)new TypeCoerceNode
                        {
                            Left = left,
                            Operator = op,
                            Type = type,
                            Alias = name
                        }
                    )
            ).Named("typeCoerce");

            var expressionEquality = Infix(
                // Equality/comparison operators
                // <typeCoerce> (<op> <typeCoerce>)+
                expressionTypeCoerce,
                Operator("==", "!=", ">=", "<=", "<", ">"),
                Required(expressionTypeCoerce, t => new EmptyNode().WithDiagnostics(t.CurrentLocation, Errors.MissingExpression)),
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
                Required(expressionEquality, t => new EmptyNode().WithDiagnostics(t.CurrentLocation, Errors.MissingExpression)),
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
                Required(expressionBitwise, t => new EmptyNode().WithDiagnostics(t.CurrentLocation, Errors.MissingExpression)),
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
                Required(expressionLogical, t => new EmptyNode().WithDiagnostics(t.CurrentLocation, Errors.MissingExpression)),
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
                            t => new EmptyNode().WithDiagnostics(t.CurrentLocation, Errors.MissingExpression)
                        ),
                        _requiredColon,
                        Required(
                            Deferred(() => _expressionConditional),
                            t => new EmptyNode().WithDiagnostics(t.CurrentLocation, Errors.MissingExpression)
                        ),
                        (condition, q, consequent, c, alternative) => (AstNode)new ConditionalNode
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
                            SeparatedList(
                                _identifiers,
                                Operator(","),
                                args => new ListNode<IdentifierNode> { Items = args.ToList(), Separator = new OperatorNode(",") }
                            ),
                            Operator(")"),
                            (a, items, c) => items.WithUnused(a, c)
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
                        Error<ListNode<AstNode>>(Errors.MissingExpression)
                    ),
                    (parameters, x, body) => (AstNode)new LambdaNode
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

        private static IParser<Token, TOutput> Error<TOutput>(string error)
            where TOutput : AstNode, new()
        {
            return Produce<Token, TOutput>(t => new TOutput().WithDiagnostics(t.CurrentLocation, error)).Named("error");
        }
    }
}

using Scoop.Parsers;
using Scoop.SyntaxTree;
using static Scoop.Parsers.ScoopParsers;

namespace Scoop
{
    public partial class Parser
    {
        public Parser()
        {
            Initialize();
        }

        public IParser<CompilationUnitNode> CompilationUnits { get; private set; }
        public IParser<TypeNode> Types { get; private set; }
        public IParser<TypeNode> DeclareTypes { get; private set; }
        public IParser<ListNode<AstNode>> ArgumentLists { get; private set; }
        public IParser<AstNode> Expressions { get; private set; }
        public IParser<ListNode<AstNode>> ExpressionList { get; private set; }
        public IParser<AstNode> Statements { get; private set; }
        public IParser<ListNode<AttributeNode>> Attributes { get; set; }
        public IParser<ListNode<IdentifierNode>> GenericTypeParameters { get; set; }
        public IParser<ListNode<TypeNode>> GenericTypeArguments { get; set; }
        public IParser<ListNode<ParameterNode>> ParameterList { get; set; }
        public IParser<ListNode<TypeConstraintNode>> TypeConstraints { get; set; }
        public IParser<DelegateNode> Delegates { get; set; }
        public IParser<EnumNode> Enums { get; set; }
        public IParser<ClassNode> Classes { get; set; }
        public IParser<AstNode> ClassMembers { get; set; }
        public IParser<InterfaceNode> Interfaces { get; set; }
        public IParser<ListNode<AstNode>> NormalMethodBody { get; set; }

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
        private IParser<AstNode> _requiredExpression;

        private IParser<AstNode> _expressions;
        private IParser<TypeNode> _types;

        private void Initialize()
        {
            Expressions = Deferred(() => _expressions).Named("Expressions");
            Types = Deferred(() => _types).Named("Types");

            _identifiers = new IdentifierParser();
            _accessModifiers = Optional(
                new KeywordParser("public", "private")
            ).Named("accessModifiers");
            _requiredSemicolon = Required(new OperatorParser(";"), Errors.MissingSemicolon);
            _requiredOpenBracket = Required(new OperatorParser("{"), Errors.MissingOpenBracket);
            _requiredCloseBracket = Required(new OperatorParser("}"), Errors.MissingCloseBracket);
            _requiredOpenParen = Required(new OperatorParser("("), Errors.MissingOpenParen);
            _requiredCloseParen = Required(new OperatorParser(")"), Errors.MissingCloseParen);
            _requiredColon = Required(new OperatorParser(":"), Errors.MissingColon);
            _requiredCloseBrace = Required(new OperatorParser("]"), Errors.MissingCloseBrace);
            _requiredCloseAngle = Required(new OperatorParser(">"), Errors.MissingCloseAngle);
            _requiredIdentifier = Required(new IdentifierParser(), Errors.MissingIdentifier);
            _requiredType = Required(Types, Errors.MissingType);
            _requiredEquals = Required(new OperatorParser("="), Errors.MissingEquals);
            _requiredExpression = Required(Expressions, () => new EmptyNode(), Errors.MissingExpression);

            InitializeTopLevel();
            InitializeTypes();
            InitializeExpressions();
            InitializeStatements();
            InitializeAttributes();
            InitializeParameters();
            InitializeDelegates();
            InitializeEnums();
            InitializeClasses();
        }
    }
}

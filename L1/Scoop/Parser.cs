using Scoop.Parsers;
using Scoop.SyntaxTree;
using Scoop.Tokenization;

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

        private void Initialize()
        {
            _identifiers = new IdentifierParser();
            _accessModifiers = ScoopParsers.Optional(
                new KeywordParser("public", "private")
            ).Named("accessModifiers");

            InitializeTopLevel();
            InitializeTypes();
            InitializeExpressions();
            InitializeStatements();
            InitializeAttributes();
            InitializeClasses();

            
        }

        private void Fail(ITokenizer t, string ruleName, Token next = null)
        {
            if (t is WindowTokenizer wt)
                wt.Rewind();
            else
                throw ParsingException.CouldNotParseRule(ruleName, next ?? t.Peek());
        }
    }
}

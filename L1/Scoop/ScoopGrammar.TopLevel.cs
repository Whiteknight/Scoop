using System.Linq;
using Scoop.Parsers;
using Scoop.SyntaxTree;
using Scoop.Tokenization;
using static Scoop.Parsers.ScoopParsers;

namespace Scoop
{
    public partial class ScoopGrammar
    {
        private void InitializeTopLevel()
        {
            // "using" <namespaceName> ";"
            var parseUsingDirective = Sequence(
                new KeywordParser("using"),
                Required(new DottedIdentifierParser(), () => new DottedIdentifierNode(""), Errors.MissingNamespaceName),
                _requiredSemicolon,
                (a, b, c) => new UsingDirectiveNode
                {
                    Location = a.Location,
                    Namespace = b
                }.WithUnused(a, c)
            );

            var namespaceMembers = First<AstNode>(
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
                Required(new DottedIdentifierParser(), () => new DottedIdentifierNode(""), Errors.MissingNamespaceName),
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

        public CompilationUnitNode ParseUnit(string s) => CompilationUnits.Parse(new Tokenizer(s)).GetResult();
        public EnumNode ParseEnum(string s) => Enums.Parse(new Tokenizer(s)).GetResult();

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
                GenericTypeParameters,
                ParameterList,
                TypeConstraints,
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
    }
}

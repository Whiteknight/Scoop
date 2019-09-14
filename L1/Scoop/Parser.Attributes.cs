using System.Linq;
using Scoop.Parsers;
using Scoop.SyntaxTree;
using Scoop.Tokenization;
using static Scoop.Parsers.ScoopParsers;

namespace Scoop
{
    public partial class Parser
    {
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
                // TODO: We could replace the Optional() call with an EmptyNode in the First()
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

        public ListNode<AttributeNode> ParseAttributes(string s) => Attributes.Parse(new Tokenizer(s)).GetResult();
    }
}

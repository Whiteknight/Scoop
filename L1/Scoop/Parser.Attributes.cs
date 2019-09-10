using System.Linq;
using Scoop.Parsers;
using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop
{
    public partial class Parser
    {
        private void InitializeAttributes()
        {
            var argumentParser = ScoopParsers.First(
                ScoopParsers.Sequence(
                    new IdentifierParser(),
                    new OperatorParser("="),
                    // TODO: I think these expressions can only be terminals or member accesses (consts or enums, etc)
                    ScoopParsers.Deferred(() => Expressions),
                    (name, s, expr) => new NamedArgumentNode { Name = name, Separator = s, Value = expr }
                ),
                // TODO: I think these expressions can only be terminals or member accesses (consts or enums, etc)
                ScoopParsers.Deferred(() => Expressions)
            );
            var argumentListParser = ScoopParsers.Optional(
                ScoopParsers.First(
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
                )
            );
            var attrParser = ScoopParsers.SeparatedList(
                ScoopParsers.Sequence(
                    ScoopParsers.Optional(
                        ScoopParsers.Sequence(
                            new KeywordParser(),
                            new OperatorParser(":"),
                            (target, o) => target
                        )
                    ),
                    ScoopParsers.Deferred(() => Types),
                    argumentListParser,
                    (target, type, args) => new AttributeNode
                    {
                        Location = type.Location,
                        Target = target as KeywordNode,
                        Type = type,
                        Arguments = args as ListNode<AstNode>
                    }
                ),
                new OperatorParser(","),
                (list) => new ListNode<AttributeNode> { Items = list.ToList(), Separator = new OperatorNode(",") }
            );
            Attributes = ScoopParsers.Transform(
                ScoopParsers.Optional(
                    ScoopParsers.List(
                        ScoopParsers.Sequence(
                            new OperatorParser("["),
                            attrParser,
                            new OperatorParser("]"),
                            (a, attrs, b) => attrs
                        ),
                        list => new ListNode<AttributeNode> { Items = list.SelectMany(l => l.Items).ToList() }
                    )
                ),
                n => n is EmptyNode ? ListNode<AttributeNode>.Default() : n as ListNode<AttributeNode>
            );
        }

        public ListNode<AttributeNode> ParseAttributes(string s) => Attributes.Parse(new Tokenizer(s)).GetResult();
    }
}

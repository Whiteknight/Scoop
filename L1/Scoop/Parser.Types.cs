using System.Linq;
using Scoop.Parsers;
using Scoop.SyntaxTree;

namespace Scoop
{
    public partial class Parser
    {
        private void InitializeTypes()
        {
            // <typeName> ("<" <typeArray> ("," <typeArray>)* ">")?
            var typeName = ScoopParsers.Transform(
                new IdentifierParser(),
                id => new TypeNode(id)
            );

            var genericType = ScoopParsers.First(
                ScoopParsers.Sequence(
                    typeName,
                    new OperatorParser("<"),
                    ScoopParsers.SeparatedList(
                        ScoopParsers.Deferred(() => Types),
                        new OperatorParser(","),
                        list => new ListNode<TypeNode> { Items = list.ToList(), Separator = new OperatorNode(",") }
                    ),
                    new OperatorParser(">"),
                    ProduceTypeNodeGenericArguments
                ),
                typeName
            );

            var subtype = ScoopParsers.SeparatedList(
                genericType,
                new OperatorParser("."),
                types => new ListNode<TypeNode> { Items = types.ToList(), Separator = new OperatorNode(".") }
            );

            // <subtype> ("[" "]")*
            Types = ScoopParsers.Sequence(
                subtype,
                ScoopParsers.List(
                    ScoopParsers.Sequence(
                        new OperatorParser("["),
                        new OperatorParser("]"),
                        (a, b) => new ArrayTypeNode { Location = a.Location }
                    ),
                    items => new ListNode<ArrayTypeNode> { Items = items.ToList() }
                ),
                ProduceTypeNodeArrayTypes
            );

            DeclareTypes = ScoopParsers.First(
                ScoopParsers.Transform(
                    new KeywordParser("var"),
                    k => new TypeNode { Name = new IdentifierNode(k.Keyword), Location = k.Location }
                ),
                ScoopParsers.Transform(
                    new KeywordParser("dynamic"),
                    k => new TypeNode { Name = new IdentifierNode(k.Keyword), Location = k.Location }
                ),
                Types
            );

            GenericTypeArguments = ScoopParsers.Sequence(
                new OperatorParser("<"),
                ScoopParsers.SeparatedList(
                    Types,
                    new OperatorParser(","),
                    types => new ListNode<TypeNode> { Items = types.ToList(), Separator = new OperatorNode(",") }
                ),
                new OperatorParser(">"),
                (a, types, b) => types
            );

            GenericTypeParameters = ScoopParsers.Transform(
                ScoopParsers.Optional(
                    ScoopParsers.Sequence(
                        new OperatorParser("<"),
                        ScoopParsers.SeparatedList(
                            new IdentifierParser(),
                            new OperatorParser(","),
                            types => new ListNode<IdentifierNode> { Items = types.ToList(), Separator = new OperatorNode(",") }
                        ),
                        new OperatorParser(">"),
                        (a, types, b) => types
                    )
                ),
                n => n is EmptyNode ? ListNode<IdentifierNode>.Default() : n as ListNode<IdentifierNode>
            );

            var constraintList = ScoopParsers.SeparatedList(
                ScoopParsers.First<AstNode>(
                    new KeywordParser("class"),
                    ScoopParsers.Sequence(
                        new KeywordParser("new"),
                        new OperatorParser("("),
                        new OperatorParser(")"),
                        (n, a, b) => new KeywordNode { Keyword = "new()", Location = n.Location }
                    ),
                    Types
                ),
                new OperatorParser(","),
                constraints => new ListNode<AstNode> { Items = constraints.ToList(), Separator = new OperatorNode(",") }
            );
            TypeConstraints = ScoopParsers.List(
                ScoopParsers.Sequence(
                    new KeywordParser("where"),
                    new IdentifierParser(),
                    new OperatorParser(":"),
                    constraintList,
                    (w, type, o, constraints) => new TypeConstraintNode
                    {
                        Location = w.Location,
                        Type = type,
                        Constraints = constraints
                    }
                ),
                allConstraints => new ListNode<TypeConstraintNode> { Items = allConstraints.ToList() }
            );
        }

        private TypeNode ProduceTypeNodeArrayTypes(ListNode<TypeNode> a, ListNode<ArrayTypeNode> b)
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
        }

        private TypeNode ProduceTypeNodeGenericArguments(TypeNode a, OperatorNode b, ListNode<TypeNode> c, OperatorNode d)
        {
            a.GenericArguments = new ListNode<TypeNode>
            {
                Items = c.Items.ToList(), Separator = c.Separator
            };
            return a;
        }
    }
}

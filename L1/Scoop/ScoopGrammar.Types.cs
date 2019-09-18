using System.Collections.Generic;
using System.Linq;
using Scoop.Parsers;
using Scoop.SyntaxTree;
using static Scoop.Parsers.ScoopParsers;

namespace Scoop
{
    public partial class ScoopGrammar
    {
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
                        ProduceGenericTypeList),
                    _requiredCloseAngle,
                    ProduceTypeNodeGenericArguments
                ),
                typeName
            ).Named("genericType");

            var subtype = SeparatedList(
                genericType,
                new OperatorParser("."),
                types => new ListNode<TypeNode> { Items = types.ToList(), Separator = new OperatorNode(".") }
            ).Named("subtype");

            // <subtype> ("[" "]")*
            _types = Sequence(
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
                ProduceTypeNodeArrayTypes
            ).Named("_types");

            DeclareTypes = First(
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

            _requiredGenericTypeArguments = Sequence(
                new OperatorParser("<"),
                SeparatedList(
                    Types,
                    new OperatorParser(","),
                    types => new ListNode<TypeNode> { Items = types.ToList(), Separator = new OperatorNode(",") },
                    atLeastOne: true
                ),
                _requiredCloseAngle,
                (a, types, b) => types.WithUnused(a, b)
            ).Named("_requiredGenericTypeArguments");
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

            GenericTypeParameters = First(
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

            TypeConstraints = List(
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

        private ListNode<TypeNode> ProduceGenericTypeList(IReadOnlyList<TypeNode> list)
        {
            var typeList = new ListNode<TypeNode>
            {
                Items = list.ToList(),
                Separator = new OperatorNode(",")
            };
            if (list.Count == 0)
                typeList.WithDiagnostics(typeList.Location, Errors.MissingType);
            return typeList;
        }

        private ListNode<IdentifierNode> ProduceGenericTypeParameterList(IReadOnlyList<IdentifierNode> types)
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

        private TypeNode ProduceTypeNodeGenericArguments(TypeNode type, OperatorNode b, ListNode<TypeNode> genericArgs, OperatorNode d)
        {
            type.GenericArguments = genericArgs;
            return type.WithUnused(b, d);
        }
    }
}

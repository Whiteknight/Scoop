using Scoop.Parsers;
using Scoop.SyntaxTree;
using static Scoop.Parsers.ScoopParsers;

namespace Scoop.Grammar
{
    public static class ScoopGrammarNamedConstructorsExtensions
    {
        public static ScoopGrammar WithNamedConstructors(this ScoopGrammar l1)
        {
            // TODO: Check if we've already added this feature to the grammar and don't
            // try to add it again.

            var compilationUnits = l1.CompilationUnits;

            // named constructors
            var identifiers = compilationUnits.FindNamed("_identifiers") as IParser<IdentifierNode>;
            var initializers = compilationUnits.FindNamed("initializers") as IParser<ListNode<AstNode>>;
            var argumentLists = compilationUnits.FindNamed("ArgumentLists") as IParser<ListNode<AstNode>>;
            var newNamed = compilationUnits.FindNamed("newNamedArgsInitsStub") as ReplaceableParser<NewNode>;
            newNamed.SetParser(
                // "new" <type> ":" <identifier> <arguments> <initializers>?
                Sequence(
                    Keyword("new"),
                    l1.Types,
                    Operator(":"),
                    identifiers,
                    argumentLists,
                    Optional(initializers),
                    (n, type, o, name, args, inits) => new NewNode
                    {
                        Location = n.Location,
                        Type = type,
                        Name = name,
                        Arguments = args,
                        Initializers = inits as ListNode<AstNode>
                    }.WithUnused(o)
                ).Named("newNamedArgsInits")
            );
            // TODO: "new" <type> ":" <identifier> <initializers>

            var accessModifiers = compilationUnits.FindNamed("accessModifiers") as IParser<AstNode>;
            var parameterLists = compilationUnits.FindNamed("ParameterList") as IParser<ListNode<ParameterNode>>;
            var methodBody = compilationUnits.FindNamed("methodBody") as IParser<ListNode<AstNode>>;
            var namedConstructor = compilationUnits.FindNamed("constructorNamedStub") as ReplaceableParser<ConstructorNode>;
            namedConstructor.SetParser(
                Sequence(
                    l1.Attributes,
                    accessModifiers,
                    identifiers,
                    Operator(":"),
                    identifiers,
                    parameterLists,
                    Optional(
                        Sequence(
                            Operator(":"),
                            Required(Keyword("this"), Errors.MissingThis),
                            argumentLists,
                            (a, b, args) => args.WithUnused(a, b)
                        )
                    ).Named("thisArgs"),
                    methodBody,
                    (attrs, vis, type, c, name, param, targs, body) => new ConstructorNode
                    {
                        Attributes = attrs.IsNullOrEmpty() ? null : attrs,
                        Location = type.Location,
                        AccessModifier = vis as KeywordNode,
                        ClassName = type,
                        Name = name,
                        Parameters = param,
                        ThisArgs = targs as ListNode<AstNode>,
                        Statements = body
                    }.WithUnused(c)
                ).Named("constructorNamed")
            );

            var newParser = compilationUnits.FindNamed("new");

            return l1;
        }
    }
}

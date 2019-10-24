using Scoop.Parsers;
using Scoop.SyntaxTree;
using Scoop.Tokenization;
using static Scoop.Parsers.ParserMethods;
using static Scoop.Parsers.TokenParserMethods;

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
            var identifiers = compilationUnits.FindNamed("_identifiers") as IParser<Token, IdentifierNode>;
            var initializers = compilationUnits.FindNamed("initializers") as IParser<Token, ListNode<AstNode>>;
            var argumentLists = compilationUnits.FindNamed("ArgumentLists") as IParser<Token, ListNode<AstNode>>;
            var newNamed = compilationUnits.FindNamed("newNamedArgsInitsStub") as ReplaceableParser<Token, NewNode>;
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
                        Initializers = inits
                    }.WithUnused(o)
                ).Named("newNamedArgsInits")
            );
            // TODO: "new" <type> ":" <identifier> <initializers>

            var accessModifiers = compilationUnits.FindNamed("accessModifiers") as IParser<Token, KeywordNode>;
            var parameterLists = compilationUnits.FindNamed("ParameterList") as IParser<Token, ListNode<ParameterNode>>;
            var methodBody = compilationUnits.FindNamed("methodBody") as IParser<Token, ListNode<AstNode>>;
            var namedConstructor = compilationUnits.FindNamed("constructorNamedStub") as ReplaceableParser<Token, ConstructorNode>;
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
                            Required(Keyword("this"), t => new KeywordNode().WithDiagnostics(t.CurrentLocation, Errors.MissingThis)),
                            argumentLists,
                            (a, b, args) => args.WithUnused(a, b)
                        )
                    ).Named("thisArgs"),
                    methodBody,
                    (attrs, vis, type, c, name, param, targs, body) => new ConstructorNode
                    {
                        Attributes = attrs.IsNullOrEmpty() ? null : attrs,
                        Location = type.Location,
                        AccessModifier = vis,
                        ClassName = type,
                        Name = name,
                        Parameters = param,
                        ThisArgs = targs,
                        Statements = body
                    }.WithUnused(c)
                ).Named("constructorNamed")
            );

            var newParser = compilationUnits.FindNamed("new");

            return l1;
        }
    }
}

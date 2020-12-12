using ParserObjects;
using ParserObjects.Parsers;
using Scoop.Parsing.Tokenization;
using Scoop.SyntaxTree;
using static ParserObjects.ParserMethods<Scoop.Parsing.Tokenization.Token>;
using static Scoop.Parsing.Parsers.TokenParserMethods;

namespace Scoop.Parsing
{
    public static class ScoopGrammarNamedConstructorsExtensions
    {
        public static ScoopGrammar WithNamedConstructors(this ScoopGrammar l1)
        {
            // TODO: Check if we've already added this feature to the grammar and don't
            // try to add it again.

            var compilationUnits = l1.CompilationUnits;

            // named constructors
            var identifiers = compilationUnits.FindNamed("_identifiers").Value as IParser<Token, IdentifierNode>;
            var initializers = compilationUnits.FindNamed("initializers").Value as IParser<Token, ListNode<AstNode>>;
            var argumentLists = compilationUnits.FindNamed("ArgumentLists").Value as IParser<Token, ListNode<AstNode>>;
            var newNamed = compilationUnits.Replace("newNamedArgsInitsStub",
                // "new" <type> ":" <identifier> <arguments> <initializers>?
                Rule(
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
                        Initializers = inits.GetValueOrDefault(null)
                    }.WithUnused(o)
                ).Named("newNamedArgsInits")
            );
            // TODO: "new" <type> ":" <identifier> <initializers>

            var accessModifiers = compilationUnits.FindNamed("accessModifiers").Value as IParser<Token, KeywordNode>;
            var parameterLists = compilationUnits.FindNamed("ParameterList").Value as IParser<Token, ListNode<ParameterNode>>;
            var methodBody = compilationUnits.FindNamed("methodBody").Value as IParser<Token, ListNode<AstNode>>;
            var namedConstructor = compilationUnits.Replace("constructorNamedStub",
                Rule(
                        l1.Attributes,
                        accessModifiers,
                        identifiers,
                        Operator(":"),
                        identifiers,
                        parameterLists,
                        Rule(
                                Operator(":"),
                                Keyword("this").Optional((t, d) => new KeywordNode().WithDiagnostics(t.CurrentLocation, Errors.MissingThis)),
                                argumentLists,
                                (a, b, args) => args.WithUnused(a, b)
                            )
                            .Optional()
                            .Named("thisArgs"),
                        methodBody,

                        (attrs, vis, type, c, name, param, targs, body) => new ConstructorNode
                        {
                            Attributes = attrs.IsNullOrEmpty() ? null : attrs,
                            Location = type.Location,
                            AccessModifier = vis,
                            ClassName = type,
                            Name = name,
                            Parameters = param,
                            ThisArgs = targs.GetValueOrDefault(null),
                            Statements = body
                        }.WithUnused(c)
                    )
                    .Named("constructorNamed")
            );

            var newParser = compilationUnits.FindNamed("new").Value;

            return l1;
        }
    }
}

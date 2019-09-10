using System.Linq;
using Scoop.Parsers;
using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop
{
    public partial class Parser
    {
        private void InitializeTopLevel()
        {
            // "using" <namespaceName> ";"
            var parseUsingDirective = ScoopParsers.Sequence(
                new KeywordParser("using"),
                new DottedIdentifierParser(),
                new OperatorParser(";"),
                (a, b, _) => new UsingDirectiveNode
                {
                    Location = a.Location,
                    Namespace = b
                });

            var namespaceMembers = ScoopParsers.First<AstNode>(
                ScoopParsers.Deferred(() => Classes),
                ScoopParsers.Deferred(() => Interfaces),
                ScoopParsers.Deferred(() => Enums),
                ScoopParsers.Deferred(() => Delegates)
            );
            var namespaceBody = ScoopParsers.First(
                ScoopParsers.Sequence(
                    new OperatorParser("{"),
                    new OperatorParser("}"),
                    (a, b) => new ListNode<AstNode>()
                ),
                ScoopParsers.Sequence(
                    new OperatorParser("{"),
                    ScoopParsers.List(
                        namespaceMembers,
                        members => new ListNode<AstNode> { Items = members.ToList() }
                    ),
                    new OperatorParser("}"),
                    (a, members, b) => members
                )
            );
            var parseNamespace = ScoopParsers.Sequence(
                // TODO: assembly-level attributes
                new KeywordParser("namespace"),
                new DottedIdentifierParser(),
                namespaceBody,
                (ns, name, members) => new NamespaceNode
                {
                    Location = ns.Location,
                    Name = name,
                    Declarations = members
                });
            CompilationUnits = ScoopParsers.Transform(
                ScoopParsers.List(
                    ScoopParsers.First<AstNode>(
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
        public CompilationUnitNode ParseUnit(ITokenizer t) => CompilationUnits.Parse(t).GetResult();
        public EnumNode ParseEnum(string s) => Enums.Parse(new Tokenizer(s)).GetResult();
    }
}

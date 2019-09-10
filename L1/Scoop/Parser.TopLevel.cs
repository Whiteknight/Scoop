using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop
{
    public partial class Parser
    {
        public CompilationUnitNode ParseUnit(string s) => ParseUnit(new Tokenizer(new StringCharacterSequence(s)));

        public CompilationUnitNode ParseUnit(ITokenizer t)
        {
            var unit = new CompilationUnitNode();
            while (true)
            {
                var lookahead = t.Peek();
                if (lookahead.Type == TokenType.EndOfInput)
                    break;
                // TODO: assembly-level attributes

                if (lookahead.IsKeyword("using"))
                {
                    var directive = ParseUsingDirective(t);
                    unit.AddUsingDirective(directive);
                    continue;
                }

                if (lookahead.IsKeyword("namespace"))
                {
                    var ns = ParseNamespace(t);
                    unit.AddNamespace(ns);
                    continue;
                }

                throw ParsingException.CouldNotParseRule(nameof(ParseUnit), lookahead);
            }

            return unit;
        }

        private NamespaceNode ParseNamespace(ITokenizer t)
        {
            var namespaceToken = t.Expect(TokenType.Keyword, "namespace");
            var namespaceNode = new NamespaceNode
            {
                Location = namespaceToken.Location,
                Name = ParseDottedIdentifier(t)
            };
            t.Expect(TokenType.Operator, "{");
            while (true)
            {
                if (t.Peek().Is(TokenType.Operator, "}"))
                    break;

                var attributes = ParseAttributes(t);
                var lookaheads = t.Peek(2);
                var lookahead = lookaheads[0].IsKeyword("public", "private") ? lookaheads[1] : lookaheads[0];

                if (lookahead.IsKeyword("class", "struct"))
                {
                    var classNode = ParseClass(t, attributes);
                    namespaceNode.AddDeclaration(classNode);
                    continue;
                }

                if (lookahead.IsKeyword("interface"))
                {
                    var ifaceNode = ParseInterface(t, attributes);
                    namespaceNode.AddDeclaration(ifaceNode);
                    continue;
                }

                if (lookahead.IsKeyword("enum"))
                {
                    var enumNode = ParseEnum(t, attributes);
                    namespaceNode.AddDeclaration(enumNode);
                    continue;
                }

                if (lookahead.IsKeyword("delegate"))
                {
                    var delegateNode = ParseDelegate(t, attributes);
                    namespaceNode.AddDeclaration(delegateNode);
                    continue;
                }

                throw ParsingException.CouldNotParseRule(nameof(ParseNamespace), lookahead);
            }
            t.Expect(TokenType.Operator, "}");
            return namespaceNode;
        }

        private UsingDirectiveNode ParseUsingDirective(ITokenizer t)
        {
            // "using" <namespaceName> ";"
            var directive = new UsingDirectiveNode
            {
                Location = t.Expect(TokenType.Keyword, "using").Location,
                Namespace = ParseDottedIdentifier(t)
            };
            t.Expect(TokenType.Operator, ";");
            return directive;
        }
    }
}

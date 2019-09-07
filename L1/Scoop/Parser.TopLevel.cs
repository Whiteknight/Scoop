using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop
{
    public partial class Parser
    {
        public CompilationUnitNode ParseUnit(string s) => ParseUnit(new Tokenizer(new StringCharacterSequence(s)));

        public CompilationUnitNode ParseUnit(Tokenizer t)
        {
            var unit = new CompilationUnitNode();
            while (true)
            {
                var lookahead = t.Peek();
                if (lookahead.Type == TokenType.EndOfInput)
                    break;

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

        private NamespaceNode ParseNamespace(Tokenizer t)
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

                var accessModifier = t.Expect(TokenType.Keyword, "public", "private");
                var lookahead = t.Peek();

                if (lookahead.IsKeyword("class"))
                {
                    t.PutBack(accessModifier);
                    var classNode = ParseClass(t);
                    namespaceNode.AddDeclaration(classNode);
                    continue;
                }

                if (lookahead.IsKeyword("interface"))
                {
                    t.PutBack(accessModifier);
                    var classNode = ParseInterface(t);
                    namespaceNode.AddDeclaration(classNode);
                    continue;
                }

                // TODO: struct, enum

                throw ParsingException.CouldNotParseRule(nameof(ParseNamespace), lookahead);
            }
            t.Expect(TokenType.Operator, "}");
            return namespaceNode;
        }

        private UsingDirectiveNode ParseUsingDirective(Tokenizer t)
        {
            // "using" <namespaceName>
            var usingToken = t.Expect(TokenType.Keyword, "using");
            var directive = new UsingDirectiveNode
            {
                Location = usingToken.Location,
                Namespace = ParseDottedIdentifier(t)
            };
            t.Expect(TokenType.Operator, ";");
            return directive;
        }
    }
}

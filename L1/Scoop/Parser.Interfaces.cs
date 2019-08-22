using System.Collections.Concurrent;
using System.Collections.Generic;
using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop
{
    public partial class Parser
    {
        // Helper method to start parsing at the interface level, mostly to simplify unit tests
        public InterfaceNode ParseInterface(string s) => ParseInterface(new Tokenizer(new StringCharacterSequence(s)));

        private InterfaceNode ParseInterface(Tokenizer t)
        {
            var accessModifierToken = t.Expect(TokenType.Keyword, "public", "private");
            t.Expect(TokenType.Keyword, "interface");
            var classNameToken = t.Expect(TokenType.Identifier);
            // TODO: ':' <ContractList>
            t.Expect(TokenType.Operator, "{");
            var memberNodes = ParseInterfaceBody(t);
            t.Expect(TokenType.Operator, "}");
            return new InterfaceNode
            {
                AccessModifier = new KeywordNode(accessModifierToken),
                Name = new IdentifierNode(classNameToken),
                Members = memberNodes
            };
        }

        private List<AstNode> ParseInterfaceBody(Tokenizer t)
        {
            // <methodSignature>*
            return new List<AstNode>();
        }
    }
}

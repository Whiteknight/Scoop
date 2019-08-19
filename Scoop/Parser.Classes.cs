using System.Collections.Generic;
using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop
{
    public partial class Parser
    {
        // Helper method to start parsing at the class level, mostly to simplify unit tests
        public ClassNode ParseClass(string s) => ParseClass(new Tokenizer(new StringCharacterSequence(s)));

        private ClassNode ParseClass(Tokenizer t)
        {
            var accessModifierToken = t.Expect(TokenType.Keyword, "public", "private");
            t.Expect(TokenType.Keyword, "class");
            var classNameToken = t.Expect(TokenType.Identifier);
            // TODO: ':' <ContractList>
            t.Expect(TokenType.Operator, "{");
            var memberNodes = ParseClassBody(t);
            t.Expect(TokenType.Operator, "}");
            return new ClassNode
            {
                AccessModifier = new KeywordNode(accessModifierToken),
                Name = new IdentifierNode(classNameToken),
                Members = memberNodes
            };
        }

        private List<AstNode> ParseClassBody(Tokenizer t)
        {
            // <ConstructorOrMethod>*

            return new List<AstNode>();
        }
    }
}

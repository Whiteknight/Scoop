using System.Collections.Generic;
using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop
{
    public partial class Parser
    {
        private AstNode ParseDelegate(ITokenizer t, List<AttributeNode> attributes = null)
        {
            // <attributes> <accessModifier>? "delegate" <type> <identifier> <genericParameters>? <parameters> <typeConstraints> ";"
            var node = new DelegateNode
            {
                Attributes = attributes ?? ParseAttributes(t),
                AccessModifier = t.Peek().IsKeyword("public", "private") ? new KeywordNode(t.GetNext()) : null,
                Location = t.Expect(TokenType.Keyword, "delegate").Location,
                ReturnType = ParseType(t),
                Name = new IdentifierNode(t.Expect(TokenType.Identifier)),
                GenericTypeParameters = ParseGenericTypeParametersList(t),
                Parameters = ParseParameterList(t),
                TypeConstraints = ParseTypeConstraints(t)
            };
            t.Expect(TokenType.Operator, ";");
            return node;
        }
    }
}

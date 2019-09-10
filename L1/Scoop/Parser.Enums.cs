using System.Collections.Generic;
using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop
{
    public partial class Parser
    {
        public EnumNode ParseEnum(string s) => ParseEnum(new Tokenizer(s), null);
        private EnumNode ParseEnum(ITokenizer t, List<AttributeNode> attributes)
        {
            var enumNode = new EnumNode
            {
                Attributes = attributes ?? ParseAttributes(t),
                AccessModifier = t.Peek().IsKeyword("public", "private") ? new KeywordNode(t.GetNext()) : null,
                Location = t.Expect(TokenType.Keyword, "enum").Location,
                Name = new IdentifierNode(t.Expect(TokenType.Identifier)),
                Members = new List<EnumMemberNode>()
            };
            t.Expect(TokenType.Operator, "{");
            if (t.NextIs(TokenType.Operator, "}", true))
                return enumNode;

            while (true)
            {
                var member = new EnumMemberNode
                {
                    Attributes = ParseAttributes(t),
                    Name = new IdentifierNode(t.Expect(TokenType.Identifier)),
                    Value = t.NextIs(TokenType.Operator, "=", true) ? new IntegerNode(t.Expect(TokenType.Integer)) : null
                };
                member.Location = member.Name.Location;
                enumNode.Members.Add(member);
                if (t.Peek().IsOperator("}"))
                    break;
                if (t.NextIs(TokenType.Operator, ",", true))
                    continue;
                throw ParsingException.CouldNotParseRule(nameof(ParseEnum), t.Peek());
            }
            t.Expect(TokenType.Operator, "}");

            return enumNode;
        }
    }
}

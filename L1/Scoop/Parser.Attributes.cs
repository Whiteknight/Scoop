using System.Collections.Generic;
using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop
{
    public partial class Parser
    {
        public List<AttributeNode> ParseAttributes(string s) => ParseAttributes(new Tokenizer(s));
        private List<AttributeNode> ParseAttributes(ITokenizer t)
        {
            if (!t.Peek().IsOperator("["))
                return null;
            var attributes = new List<AttributeNode>();
            while (t.NextIs(TokenType.Operator, "[", true))
            {
                while (true)
                {
                    var attr = new AttributeNode();
                    var lookaheads = t.Peek(2);
                    if ((lookaheads[0].IsKeyword() || lookaheads[0].IsType(TokenType.Identifier)) && lookaheads[1].IsOperator(":"))
                    {
                        t.Advance(2);
                        attr.Target = new KeywordNode(lookaheads[0]);
                    }

                    attr.Type = ParseType(t);
                    attr.Location = attr.Type.Location;
                    attr.Arguments = ParseAttributeArgumentList(t);
                    attributes.Add(attr);
                    if (t.NextIs(TokenType.Operator, ",", true))
                        continue;
                    if (t.NextIs(TokenType.Operator, "]"))
                        break;

                    Fail(t, nameof(ParseAttributes));
                    return null;
                }

                t.Expect(TokenType.Operator, "]");
            }

            return attributes;
        }

        private List<AstNode> ParseAttributeArgumentList(ITokenizer t)
        {
            if (!t.NextIs(TokenType.Operator, "(", true))
                return null;
            var args = new List<AstNode>();
            while (true)
            {
                var lookaheads = t.Peek(2);
                if (lookaheads[0].IsOperator(")"))
                    break;
                if (lookaheads[0].IsType(TokenType.Identifier) && lookaheads[1].IsOperator("="))
                {
                    var arg = new NamedArgumentNode
                    {
                        Name = new IdentifierNode(t.Expect(TokenType.Identifier)),
                        Separator = new OperatorNode(t.Expect(TokenType.Operator, "=")),
                        // TODO: I think these expressions can only be terminals or member accesses (consts or enums, etc)
                        Value = ParseExpression(t)
                    };
                    arg.Location = arg.Name.Location;
                    args.Add(arg);
                }
                else
                {
                    // TODO: I think these expressions can only be terminals or member accesses (consts or enums, etc)
                    var arg = ParseExpression(t);
                    args.Add(arg);
                }
                if (t.NextIs(TokenType.Operator, ",", true))
                    continue;

                break;
            }

            t.Expect(TokenType.Operator, ")");
            return args;
        }
    }
}

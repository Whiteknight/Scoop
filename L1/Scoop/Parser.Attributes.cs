using System.Collections.Generic;
using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop
{
    public partial class Parser
    {
        private List<AttributeNode> ParseAttributes(Tokenizer t)
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

                    throw ParsingException.CouldNotParseRule(nameof(ParseAttributes), t.Peek());
                }

                t.Expect(TokenType.Operator, "]");
            }

            return attributes;
        }

        private List<AstNode> ParseAttributeArgumentList(Tokenizer t)
        {
            if (!t.NextIs(TokenType.Operator, "(", true))
                return null;
            var args = new List<AstNode>();
            while (true)
            {
                var lookahead = t.Peek();
                if (lookahead.IsOperator(")"))
                    break;
                // TODO: <property> "=" <expr>
                // TODO: I think these expressions can only be terminals or member accesses (consts or enums, etc)
                var arg = ParseExpressionLambda(t);
                args.Add(arg);
                if (t.Peek().IsOperator(","))
                {
                    t.Advance();
                    continue;
                }

                break;
            }

            t.Expect(TokenType.Operator, ")");
            return args;
        }
    }
}

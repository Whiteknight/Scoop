using System.Collections.Generic;
using System.Linq;
using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop
{
    public partial class Parser
    {
        private DottedIdentifierNode ParseDottedIdentifier(ITokenizer t)
        {
            // <identifier> ("." <identifier>)*
            var tokens = new List<Token>();
            var id = t.Expect(TokenType.Identifier);
            tokens.Add(id);
            while (t.NextIs(TokenType.Operator, ".", true))
            {
                id = t.Expect(TokenType.Identifier);
                tokens.Add(id);
            }

            return new DottedIdentifierNode(tokens.Select(x => x.Value), tokens[0].Location);
        }
    }
}

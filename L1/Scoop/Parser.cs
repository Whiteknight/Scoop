using Scoop.Tokenization;

namespace Scoop
{
    public partial class Parser
    {
        private void Fail(ITokenizer t, string ruleName, Token next = null)
        {
            if (t is WindowTokenizer wt)
                wt.Rewind();
            else
                throw ParsingException.CouldNotParseRule(ruleName, next ?? t.Peek());
        }
    }
}

using Scoop.Tokenization;

namespace Scoop.Tests.Utility
{
    public static class TokenExtensions
    {
        public static TokenAssertions Should(this Token token)
        {
            return new TokenAssertions(token);
        }
    }
}
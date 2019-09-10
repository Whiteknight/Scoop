using System.Collections.Generic;

namespace Scoop.Tokenization
{
    public interface ITokenizer
    {
        void PutBack(Token token);
        Token ScanNextToken();
    }
}
namespace Scoop.Tokenization
{
    public interface ITokenizer
    {
        void PutBack(Token token);
        Token ScanNextToken();
    }
}
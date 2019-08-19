namespace Scoop.Tokenization
{
    public interface ICharacterSequence
    {
        char GetNext();
        void PutBack(char c);
        char Peek();
        Location GetLocation();
        char Expect(char expected);
    }
}
namespace Scoop.Tokenization
{
    public interface ICharacterSequence
    {
        char GetNext();
        void PutBack(char c);
        Location GetLocation();
    }

    public static class CharacterSequenceExtensions
    {
        public static char Peek(this ICharacterSequence cs)
        {
            var c = cs.GetNext();
            cs.PutBack(c);
            return c;
        }

        public static char Expect(this ICharacterSequence cs, char expected)
        {
            var c = cs.GetNext();
            if (c != expected)
                throw ParsingException.UnexpectedCharacter(expected, c, cs.GetLocation());
            return c;
        }
    }
}
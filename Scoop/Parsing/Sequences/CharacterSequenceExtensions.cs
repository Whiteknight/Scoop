using Scoop.Parsing.Tokenization;

namespace Scoop.Parsing.Sequences
{
    public static class CharacterSequenceExtensions
    {
        public static char Expect(this ISequence<char> cs, char expected)
        {
            var c = cs.GetNext();
            if (c != expected)
                throw TokenizingException.UnexpectedCharacter(expected, c, cs.CurrentLocation);
            return c;
        }
    }
}
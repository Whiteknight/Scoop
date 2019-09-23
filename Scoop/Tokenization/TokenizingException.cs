using System;
using System.Runtime.Serialization;

namespace Scoop.Tokenization
{
    /// <summary>
    /// Exception to communicate errors by the Tokenizer to craft a Token
    /// Because these exceptions relate to the contents of the input document, they are typically not recoverable
    /// by the parser or the downstream software.
    /// </summary>
    [Serializable]
    public class TokenizingException : Exception
    {
        public TokenizingException()
        {
        }

        public TokenizingException(string message) : base(message)
        {
        }

        public TokenizingException(string message, Exception inner) : base(message, inner)
        {
        }

        protected TokenizingException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

        public static TokenizingException UnexpectedEndOfInput(Location l)
        {
            return new TokenizingException($"Unexpected end of input or unterminated production sequence at {l}");
        }

        public static TokenizingException UnexpectedCharacter(char expected, char found, Location l)
        {
            return new TokenizingException($"Expected {expected} but found {found} at {l}");
        }

        public static TokenizingException UnexpectedCharacter(char found, Location l)
        {
            return new TokenizingException($"Unexpected character {found} at {l}");
        }
    }
}
using System.Linq;

namespace Scoop.Tokenization
{
    public class Token
    {
        public Token(string value, TokenType type, Location location)
        {
            Value = value;
            Type = type;
            Location = location;
        }

        public string Value { get; }
        public Location Location { get; }
        public TokenType Type { get; }

        public static Token EndOfInput() => new Token(null, TokenType.EndOfInput, null);

        public static Token Whitespace(string s, Location l) => new Token(s, TokenType.Whitespace, l);

        public static Token Comment(string s, Location l) => new Token(s, TokenType.Comment, l);

        public static Token Word(string s, Location l) => new Token(s, TokenType.Word, l);

        public static Token String(string s, Location l) => new Token(s, TokenType.String, l);

        public static Token Character(string s, Location l) => new Token(s, TokenType.Character, l);

        public static Token Operator(string s, Location l) => new Token(s, TokenType.Operator, l);

        public static Token CSharpLiteral(string s, Location l) => new Token(s, TokenType.CSharpLiteral, l);

        public bool IsType(params TokenType[] types) => types.Any(t => Type == t);

        public bool Is(TokenType type, string value) => Type == type && Value == value;

        public bool IsOperator(params string[] operators)
        {
            if (Type != TokenType.Operator)
                return false;
            foreach (var op in operators)
            {
                if (Value == op)
                    return true;
            }

            return false;
        }

        public override string ToString()
        {
            return $"{Type}:{Value}";
        }
    }
}
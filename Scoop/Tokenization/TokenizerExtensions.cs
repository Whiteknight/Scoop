namespace Scoop.Tokenization
{
    public static class TokenizerExtensions
    {
        public static Token GetNext(this ITokenizer tokenizer, bool skipComments = true)
        {
            while (true)
            {
                var next = tokenizer.ScanNextToken();
                if (next == null || next.IsType(TokenType.EndOfInput))
                    return Token.EndOfInput();
                if (next.Type == TokenType.Comment && skipComments)
                    continue;
                if (next.Type == TokenType.Whitespace)
                    continue;
                return next;
            }
        }

        public static void Advance(this ITokenizer tokenizer) => GetNext(tokenizer);

        public static Token Peek(this ITokenizer tokenizer)
        {
            var t = tokenizer.GetNext();
            tokenizer.PutBack(t);
            return t;
        }

        public static ITokenizer Mark(this ITokenizer tokenizer) => new WindowTokenizer(tokenizer);
    }
}
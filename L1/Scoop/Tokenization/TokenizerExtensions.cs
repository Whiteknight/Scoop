namespace Scoop.Tokenization
{
    public static class TokenizerExtensions
    {
        public static Token GetNext(this Tokenizer tokenizer, bool skipComments = true)
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

        public static bool NextIs(this Tokenizer tokenizer, TokenType type, string value, bool consume = false)
        {
            var t = tokenizer.GetNext();
            bool isSame = t.Type == type && t.Value == value;
            if (!isSame)
            {
                tokenizer.PutBack(t);
                return false;
            }

            if (!consume)
                tokenizer.PutBack(t);
            return true;
        }

        public static Token Expect(this Tokenizer tokenizer, TokenType type)
        {
            var found = tokenizer.GetNext();
            if (found.Type != type)
                throw ParsingException.UnexpectedToken(type, found);
            return found;
        }

        public static Token Expect(this Tokenizer tokenizer, TokenType type, params string[] values)
        {
            var found = tokenizer.GetNext();
            if (found.Type == type)
            {
                foreach (var value in values)
                {
                    if (found.Value == value)
                        return found;
                }
            }

            throw ParsingException.UnexpectedToken(type, values, found);
        }

        public static Token Peek(this Tokenizer tokenizer)
        {
            var t = tokenizer.GetNext();
            tokenizer.PutBack(t);
            return t;
        }

        public static Token ExpectPeek(this Tokenizer tokenizer, TokenType type)
        {
            var found = tokenizer.Peek();
            if (found.Type != type)
                throw ParsingException.UnexpectedToken(type, found);
            return found;
        }

        public static void Skip(this Tokenizer tokenizer, TokenType type)
        {
            while (true)
            {
                var t = tokenizer.GetNext();
                if (t.Type == TokenType.EndOfInput || type == TokenType.EndOfInput)
                    break;
                if (t.Type != type)
                {
                    tokenizer.PutBack(t);
                    break;
                }
            }
        }

        //public static Token MaybeGetKeywordSequence(this Tokenizer tokenizer, params string[] allowed)
        //{
        //    var lookup = new HashSet<string>(allowed);
        //    var keywords = new List<Token>();
        //    while (true)
        //    {
        //        var next = tokenizer.GetNext();
        //        if (!next.IsType(TokenType.Keyword))
        //        {
        //            tokenizer.PutBack(next);
        //            break;
        //        }
        //        if (!lookup.Contains(next.Value))
        //        {
        //            tokenizer.PutBack(next);
        //            break;
        //        }
        //        keywords.Add(next);
        //    }

        //    if (!keywords.Any())
        //        return null;

        //    var combined = string.Join(" ", keywords.Select(k => k.Value));
        //    return Token.Keyword(combined, keywords.First().Location);
        //}

        public static Token GetIdentifierOrKeyword(this Tokenizer tokenizer)
        {
            var next = tokenizer.GetNext();
            if (next.IsType(TokenType.Identifier) || next.IsType(TokenType.Keyword))
                return next;
            throw ParsingException.UnexpectedToken(TokenType.Identifier, next);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using Scoop.Parsers;

namespace Scoop.Tokenization
{
    public class Tokenizer : IEnumerable<Token>, ISequence<Token>
    {
        private readonly ISequence<char> _chars;
        private readonly Stack<Token> _putbacks;
        private readonly IParser<char, Token> _scanner;

        public Tokenizer(string s)
            : this(new StringCharacterSequence(s ?? ""))
        {
        }

        public Tokenizer(ISequence<char> chars)
        {
            _chars = chars;
            _scanner = LexicalGrammar.GetParser();
            _putbacks = new Stack<Token>();
        }

        public Tokenizer(ISequence<char> chars, IParser<char, Token> scanner)
        {
            _chars = chars;
            _scanner = scanner;
        }


        public Token GetNext()
        {
            while (true)
            {
                var next = ScanNextToken();
                if (next == null || next.IsType(TokenType.EndOfInput))
                    return Token.EndOfInput();
                if (next.Type == TokenType.Comment || next.Type == TokenType.Whitespace)
                    continue;
                return next;
            }
        }

        public Token Peek()
        {
            if (_putbacks.Count > 0)
                return _putbacks.Peek();
            var next = GetNext();
            PutBack(next);
            return next;
        }

        public Location CurrentLocation => Peek().Location;

        private Token ScanNextToken()
        {
            if (_putbacks.Count > 0)
                return _putbacks.Pop();

            var next = _scanner.Parse(_chars);
            if (!next.Success || next.Value == null)
                return Token.EndOfInput();
            return next.Value;
        }

        public void PutBack(Token token)
        {
            if (token != null)
                _putbacks.Push(token);
        }

        public IEnumerator<Token> GetEnumerator()
        {
            while (true)
            {
                var next = GetNext();
                if (next.IsType(TokenType.EndOfInput))
                    yield break;
                yield return next;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
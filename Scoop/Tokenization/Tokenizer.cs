using System;
using System.Collections;
using System.Collections.Generic;

namespace Scoop.Tokenization
{
    public class Tokenizer : IEnumerable<Token>, ISequence<Token>
    {
        private readonly Stack<Token> _putbacks;
        private readonly TokenScanner _scanner;

        public Tokenizer(string s)
            : this(new StringCharacterSequence(s ?? ""))
        {
        }

        public Tokenizer(ICharacterSequence chars)
            : this(new TokenScanner(chars))
        {
        }

        public Tokenizer(TokenScanner scanner)
        {
            _putbacks = new Stack<Token>();
            _scanner = scanner ?? throw new ArgumentNullException(nameof(scanner));
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

            var next = _scanner.ScanNext();
            if (next == null)
                return Token.EndOfInput();
            return next;
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
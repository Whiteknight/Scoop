using System;
using System.Collections;
using System.Collections.Generic;

namespace Scoop.Tokenization
{
    public class Tokenizer : IEnumerable<Token>, ITokenizer
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

        public Token ScanNextToken()
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
                var next = _scanner.ScanNext();
                if (next == null || next.IsType(TokenType.EndOfInput))
                    break;
                yield return next;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
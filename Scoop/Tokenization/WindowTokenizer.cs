using System.Collections.Generic;

namespace Scoop.Tokenization
{
    public class WindowTokenizer : ITokenizer
    {
        private readonly Stack<Token> _window;
        private readonly ITokenizer _inner;

        public WindowTokenizer(ITokenizer inner)
        {
            _inner = inner;
            _window = new Stack<Token>();
        }

        public void PutBack(Token token)
        {
            if (_window.Peek() == token)
                _window.Pop();
            _inner.PutBack(token);
        }

        public Token ScanNextToken()
        {
            var token = _inner.ScanNextToken();
            _window.Push(token);
            return token;
        }

        public void Rewind()
        {
            while (_window.Count > 0)
                _inner.PutBack(_window.Pop());
        }
    }
}

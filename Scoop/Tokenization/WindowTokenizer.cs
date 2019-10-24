using System.Collections.Generic;

namespace Scoop.Tokenization
{
    public class WindowTokenizer<T> : ISequence<T>
    {
        private readonly Stack<T> _window;
        private readonly ISequence<T> _inner;

        public WindowTokenizer(ISequence<T> inner)
        {
            _inner = inner;
            _window = new Stack<T>();
        }

        public void PutBack(T token)
        {
            if (_window.Peek().Equals(token))
                _window.Pop();
            _inner.PutBack(token);
        }

        public void Rewind()
        {
            while (_window.Count > 0)
                _inner.PutBack(_window.Pop());
        }

        public T GetNext()
        {
            var token = _inner.GetNext();
            _window.Push(token);
            return token;
        }

        public T Peek()
        {
            return _inner.Peek();
        }

        public Location CurrentLocation => _inner.CurrentLocation;
    }
}

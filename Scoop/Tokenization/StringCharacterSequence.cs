﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Scoop.Tokenization
{
    public class StringCharacterSequence : ICharacterSequence
    {
        private readonly string _s;
        private readonly Stack<char> _putbacks;
        private int _index;
        private int _line;
        private int _column;

        public StringCharacterSequence(string s)
        {
            _s = s;
            _putbacks = new Stack<char>();
        }

        public char GetNext()
        {
            if (_putbacks.Any())
                return _putbacks.Pop();
            if (_index >= _s.Length)
                return '\0';
            var next = _s[_index++];
            if (next == '\n')
            {
                _line++;
                _column = 0;
            }
            else
                _column++;

            return next;
        }

        public void PutBack(char c)
        {
            // TODO: We need to reset _column to end-of-line
            if (c == '\n')
                _line--;
            _putbacks.Push(c);
        }

        public char Peek()
        {
            var c = GetNext();
            PutBack(c);
            return c;
        }

        public char Expect(char expected)
        {
            var c = GetNext();
            if (c != expected)
                throw ParsingException.UnexpectedCharacter(expected, c, GetLocation());
            return c;
        }

        public Location GetLocation()
        {
            return new Location(_line, _column);
        }
    }
}
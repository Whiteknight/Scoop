using System.Collections.Generic;
using Scoop.Parsers;

namespace Scoop.Tokenization
{
    public partial class TokenParser
    {
        private IParseResult<Token> ReadString(ISequence<char> _chars)
        {
            var buffer = new List<char>();
            var l = _chars.CurrentLocation;
            AdvanceThroughString(_chars, buffer);
            var str = new string(buffer.ToArray());
            return new Result<Token>(true, Token.String(str, l));
        }

        private void AdvanceThroughString(ISequence<char> _chars, List<char> buffer)
        {
            var c = _chars.GetNext();
            if (c == '$')
            {
                buffer.Add(c);
                var d = _chars.Peek();
                if (d == '@')
                {
                    buffer.Add(_chars.GetNext());
                    buffer.Add(_chars.Expect('"'));
                    AdvanceThroughString(_chars, StringReadState.InterpolatedBlockString, buffer);
                    return;
                }

                buffer.Add(_chars.Expect('"'));
                AdvanceThroughString(_chars, StringReadState.InterpolatedString, buffer);
                return;
            }

            if (c == '@')
            {
                buffer.Add(c);
                buffer.Add(_chars.Expect('"'));
                AdvanceThroughString(_chars, StringReadState.BlockString, buffer);
                return;
            }

            if (c == '"')
            {
                buffer.Add(c);
                AdvanceThroughString(_chars, StringReadState.String, buffer);
                return;
            }

            throw TokenizingException.UnexpectedCharacter('"', c, _chars.CurrentLocation);
        }

        private enum StringReadState
        {
            End,
            Brackets,
            String,
            BlockString,
            InterpolatedString,
            InterpolatedBlockString,
        }

        // This method is a large mess. We can consider breaking it up into a proper state machine
        // with classes per state
        private void AdvanceThroughString(ISequence<char> _chars, StringReadState initialState, List<char> chars)
        {
            var l = _chars.CurrentLocation;
            var nesting = new Stack<StringReadState>();
            nesting.Push(StringReadState.End);
            var current = initialState;

            void PushState(StringReadState next)
            {
                nesting.Push(current);
                current = next;
            }

            void PopState()
            {
                current = nesting.Pop();
            }

            while (true)
            {
                if (current == StringReadState.End)
                    return;
                var c = _chars.GetNext();
                if (c == '\0')
                    throw TokenizingException.UnexpectedEndOfInput(l);
                chars.Add(c);

                switch (current)
                {
                    case StringReadState.Brackets:
                        // basic state-machine to try and scan through this block
                        // TODO: I think it's possible for the expression to contain a {} pair, so we should 
                        // count the number of brackets and only PopState() when we've hit 0
                        if (c == '}')
                        {
                            PopState();
                            continue;
                        }

                        if (c == '$')
                        {
                            var d = _chars.GetNext();
                            if (d == '"')
                            {
                                chars.Add(d);
                                PushState(StringReadState.InterpolatedString);
                                continue;
                            }

                            if (d == '@' && _chars.Peek() == '"')
                            {
                                chars.Add(d);
                                chars.Add(_chars.GetNext());
                                PushState(StringReadState.InterpolatedBlockString);
                            }

                            continue;
                        }

                        if (c == '@' && _chars.Peek() == '"')
                        {
                            chars.Add(_chars.GetNext());
                            PushState(StringReadState.BlockString);
                            continue;
                        }

                        if (c == '"')
                        {
                            PushState(StringReadState.String);
                            continue;
                        }

                        break;
                    case StringReadState.String:
                        if (c == '\\')
                        {
                            chars.Add(_chars.GetNext());
                            continue;
                        }

                        if (c == '"')
                        {
                            PopState();
                            continue;
                        }

                        break;
                    case StringReadState.InterpolatedString:
                        if (c == '\\')
                        {
                            chars.Add(_chars.GetNext());
                            continue;
                        }
                        if (c == '{')
                        {
                            PushState(StringReadState.Brackets);
                            continue;
                        }

                        if (c == '"')
                        {
                            PopState();
                            continue;
                        }

                        break;
                    case StringReadState.BlockString:
                        if (c == '"')
                        {
                            var d = _chars.GetNext();
                            if (d == '"')
                            {
                                chars.Add(d);
                                continue;
                            }

                            _chars.PutBack(d);
                            PopState();
                            continue;
                        }

                        break;
                    case StringReadState.InterpolatedBlockString:
                        if (c == '"')
                        {
                            var d = _chars.GetNext();
                            if (d == '"')
                            {
                                chars.Add(d);
                                continue;
                            }

                            _chars.PutBack(d);
                            PopState();
                            continue;
                        }
                        if (c == '{')
                        {
                            PushState(StringReadState.Brackets);
                            continue;
                        }

                        break;
                }
            }
        }
    }
}

using System.Collections.Generic;

namespace Scoop.Tokenization
{
    public partial class TokenScanner
    {
        private Token ReadString()
        {
            var buffer = new List<char>();
            var l = _chars.GetLocation();
            AdvanceThroughString(buffer);
            var str = new string(buffer.ToArray());
            return Token.String(str, l);
        }

        private void AdvanceThroughString(List<char> buffer)
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
                    AdvanceThroughString(StringReadState.InterpolatedBlockString, buffer);
                    return;
                }

                buffer.Add(_chars.Expect('"'));
                AdvanceThroughString(StringReadState.InterpolatedString, buffer);
                return;
            }

            if (c == '@')
            {
                buffer.Add(c);
                buffer.Add(_chars.Expect('"'));
                AdvanceThroughString(StringReadState.BlockString, buffer);
                return;
            }

            if (c == '"')
            {
                buffer.Add(c);
                AdvanceThroughString(StringReadState.String, buffer);
                return;
            }

            throw ParsingException.UnexpectedCharacter('"', c, _chars.GetLocation());
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
        private void AdvanceThroughString(StringReadState initialState, List<char> chars)
        {
            var l = _chars.GetLocation();
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
                    throw ParsingException.UnexpectedEndOfInput(l);
                chars.Add(c);

                switch (current)
                {
                    case StringReadState.Brackets:
                        // basic state-machine to try and scan through this block
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

using System.Collections.Generic;
using System.Linq;
using Scoop.Parsing.Parsers;
using Scoop.Parsing.Sequences;

namespace Scoop.Parsing.Tokenization.Parsers
{
    public class StringParser : IParser<char, Token>
    {
        public IParseResult<Token> Parse(ISequence<char> t)
        {
            var buffer = new List<char>();
            bool ok = AdvanceThroughString(t, buffer);
            if (!ok)
                return Result<Token>.Fail();

            var str = new string(buffer.ToArray());
            return new Result<Token>(true, Token.String(str));
        }

        public IParseResult<object> ParseUntyped(ISequence<char> t) => Parse(t);

        public string Name { get; set; }

        public IParser Accept(IParserVisitorImplementation visitor) => this;

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public IParser ReplaceChild(IParser find, IParser replace) => this;

        private bool AdvanceThroughString(ISequence<char> _chars, List<char> buffer)
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
                    return true;
                }

                buffer.Add(_chars.Expect('"'));
                AdvanceThroughString(_chars, StringReadState.InterpolatedString, buffer);
                return true;
            }

            if (c == '@')
            {
                buffer.Add(c);
                buffer.Add(_chars.Expect('"'));
                AdvanceThroughString(_chars, StringReadState.BlockString, buffer);
                return true;
            }

            if (c == '"')
            {
                buffer.Add(c);
                AdvanceThroughString(_chars, StringReadState.String, buffer);
                return true;
            }

            _chars.PutBack(c);
            return false;
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
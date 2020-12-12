using System.Collections.Generic;
using System.Linq;
using ParserObjects;

namespace Scoop.Parsing.Tokenization.Parsers
{
    public class StringParser : IParser<char, Token>
    {
        private static readonly HashSet<char> _hexChars = new HashSet<char>("abcdefABCDEF0123456789");
        private static readonly HashSet<char> _escapeChars = new HashSet<char>("abfnrtv0'\"\\");

        public IResult<Token> Parse(ParseState<char> state)
        {
            int startConsumed = state.Input.Consumed;
            var startLocation = state.Input.CurrentLocation;
            var token = ReadStringToken(state.Input);
            if (token == null)
                return state.Fail(this, "");
            return state.Success(this, token, state.Input.Consumed - startConsumed, startLocation);
        }

        public string Name { get; set; }

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public IParser ReplaceChild(IParser find, IParser replace) => this;

        private Token ReadStringToken(ISequence<char> t)
        {
            var buffer = new List<char>();
            // TODO: Don't .Expect chars here. If we don't see something we want, back out and fail
            // TODO: C# 8.0 allows "$" and "@" to appear in any order
            var c = t.GetNext();
            if (c == '$')
            {
                buffer.Add(c);
                var d = t.GetNext();
                if (d == '@')
                {
                    buffer.Add(d);
                    var e = t.GetNext();
                    if (e == '"')
                    {
                        buffer.Add(e);
                        return AdvanceThroughString(t, StringReadState.InterpolatedBlockString, buffer);
                    }

                    return Token.String(new string(buffer.ToArray())).WithDiagnostics(Errors.MissingDoubleQuote);
                }

                if (d == '"')
                {
                    buffer.Add(d);
                    return AdvanceThroughString(t, StringReadState.InterpolatedString, buffer);
                }

                return Token.String(new string(buffer.ToArray())).WithDiagnostics(Errors.MissingDoubleQuote);
            }

            if (c == '@')
            {
                buffer.Add(c);
                var d = t.GetNext();
                if (d == '"')
                {
                    buffer.Add(d);
                    return AdvanceThroughString(t, StringReadState.BlockString, buffer);
                }

                return Token.String(new string(buffer.ToArray())).WithDiagnostics(Errors.MissingDoubleQuote);
            }

            if (c == '"')
            {
                buffer.Add(c);
                return AdvanceThroughString(t, StringReadState.String, buffer);
            }

            t.PutBack(c);
            return null;
        }

        private enum StringReadState
        {
            // At the end of the string
            End,

            // Inside {} brackes for interpolation
            Brackets,

            // Inside a normal string
            String,

            // Inside a @ string
            BlockString,

            // Inside a $ interpolated string
            InterpolatedString,

            // Inside a @$ block-interpolated string
            InterpolatedBlockString,
        }

        // This method is a large mess. We can consider breaking it up into a proper state machine
        // with classes per state
        private Token AdvanceThroughString(ISequence<char> t, StringReadState initialState, List<char> buffer)
        {
            var nesting = new Stack<StringReadState>();
            nesting.Push(StringReadState.End);
            var current = initialState;
            var errors = new List<Diagnostic>();

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
                // We're at the end of the string successfully, return the token
                if (current == StringReadState.End)
                    return Token.String(new string(buffer.ToArray())).WithDiagnostics(errors);

                var c = t.GetNext();
                if (c == '\0')
                {
                    errors.Add(new Diagnostic(Errors.UnexpectedEndOfInput, t.CurrentLocation));
                    return Token.String(new string(buffer.ToArray())).WithDiagnostics(errors);
                }

                buffer.Add(c);

                switch (current)
                {
                    case StringReadState.Brackets:
                        // basic state-machine to try and scan through this block
                        // It's possible to have {} pairs inside brackets, so we'll push them to keep track
                        // of balance on the stack.
                        if (c == '{')
                        {
                            PushState(StringReadState.Brackets);
                            continue;
                        }
                        if (c == '}')
                        {
                            PopState();
                            continue;
                        }

                        if (c == '$')
                        {
                            var d = t.GetNext();
                            if (d == '"')
                            {
                                buffer.Add(d);
                                PushState(StringReadState.InterpolatedString);
                                continue;
                            }

                            if (d == '@' && t.Peek() == '"')
                            {
                                buffer.Add(d);
                                buffer.Add(t.GetNext());
                                PushState(StringReadState.InterpolatedBlockString);
                                continue;
                            }

                            errors.Add(new Diagnostic(Errors.UnexpectedToken, t.CurrentLocation));
                            continue;
                        }

                        if (c == '@' && t.Peek() == '"')
                        {
                            buffer.Add(t.GetNext());
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
                            AdvanceThroughSlashEscapeSequence(t, buffer, errors);
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
                            AdvanceThroughSlashEscapeSequence(t, buffer, errors);
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
                            var d = t.GetNext();
                            if (d == '"')
                            {
                                buffer.Add(d);
                                continue;
                            }

                            t.PutBack(d);
                            PopState();
                            continue;
                        }

                        break;

                    case StringReadState.InterpolatedBlockString:
                        if (c == '"')
                        {
                            var d = t.GetNext();
                            if (d == '"')
                            {
                                buffer.Add(d);
                                continue;
                            }

                            t.PutBack(d);
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

        private static void AdvanceThroughSlashEscapeSequence(ISequence<char> t, List<char> buffer, List<Diagnostic> errors)
        {
            var d = t.GetNext();
            buffer.Add(d);

            // "\" "x" <hexChar>{1, 4}
            if (d == 'x')
            {
                AdvanceThroughHexEscapeSequence(t, buffer, errors);
                return;
            }

            // "\" "u" <hexChar>{4}
            if (d == 'u')
            {
                AdvanceThroughShortUnicodeEscapeSequence(t, buffer, errors);
                return;
            }

            // "\" "u" <hexChar>{8}
            if (d == 'U')
            {
                AdvanceThroughLongUnicodeEscapeSequence(t, buffer, errors);
                return;
            }

            if (_escapeChars.Contains(d))
                return;

            errors.Add(new Diagnostic(Errors.UnrecognizedCharEscape, t.CurrentLocation));
        }

        private static void AdvanceThroughLongUnicodeEscapeSequence(ISequence<char> t, List<char> buffer, List<Diagnostic> errors)
        {
            // TODO: Should we test for a valid sequence?
            // \U can go from 00000000-0010FFFF (and I think there's a range or two in there which are also invalid)
            // Should we check for formatting and valid values or just leave it for Roslyn?
            for (int i = 0; i < 8; i++)
            {
                var e = t.GetNext();
                buffer.Add(e);
                if (!_hexChars.Contains(e))
                {
                    t.PutBack(e);
                    errors.Add(new Diagnostic(Errors.UnrecognizedCharEscape, t.CurrentLocation));
                    break;
                }
            }
        }

        private static void AdvanceThroughShortUnicodeEscapeSequence(ISequence<char> t, List<char> buffer, List<Diagnostic> errors)
        {
            for (int i = 0; i < 4; i++)
            {
                var e = t.GetNext();
                buffer.Add(e);
                if (!_hexChars.Contains(e))
                {
                    t.PutBack(e);
                    errors.Add(new Diagnostic(Errors.UnrecognizedCharEscape, t.CurrentLocation));
                    break;
                }
            }
        }

        private static void AdvanceThroughHexEscapeSequence(ISequence<char> t, List<char> buffer, List<Diagnostic> errors)
        {
            var e = t.GetNext();
            buffer.Add(e);
            if (!_hexChars.Contains(e))
            {
                errors.Add(new Diagnostic(Errors.UnrecognizedCharEscape, t.CurrentLocation));
                return;
            }

            for (int i = 0; i < 3; i++)
            {
                e = t.GetNext();
                if (!_hexChars.Contains(e))
                {
                    t.PutBack(e);
                    break;
                }

                buffer.Add(e);
            }
        }

        IResult IParser<char>.Parse(ParseState<char> state) => Parse(state);
    }
}

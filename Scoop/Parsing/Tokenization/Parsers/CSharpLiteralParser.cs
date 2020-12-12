using System.Collections.Generic;
using System.Linq;
using ParserObjects;
using Scoop.Parsing.Sequences;

namespace Scoop.Parsing.Tokenization.Parsers
{
    public class CSharpLiteralParser : IParser<char, Token>
    {
        public IResult<Token> Parse(ParseState<char> state)
        {
            var input = state.Input;
            // Attempt to read through an arbitrary c# code literal. We can largely do this by
            // counting braces, but we have to get a bit more involved when we deal with
            // braces which are quoted as chars and strings: '{' and "{".

            // C# is a complicated language and this parser does not want to be complicated. It is expected
            // that the verbatim contents of this block are passed, opaquely, to roslyn for compilation.
            // This parser will not attempt in any way to recognize the contents of the C# block or provide
            // helpful diagnostics/recovery. If the user doesn't balance their {} brackets, they're going
            // to get an exception and a hard stop.
            var startConsumed = input.Consumed;
            var a = input.GetNext();
            var b = input.Peek();
            if (a != 'c' || b != '#')
            {
                input.PutBack(a);
                return state.Fail(this, "");
            }

            input.GetNext();

            while (char.IsWhiteSpace(input.Peek()))
                input.GetNext();
            input.Expect('{');
            int braceCount = 1;
            var buffer = new List<char>();
            while (true)
            {
                var c = input.GetNext();
                if (c == '\0')
                    TokenizingException.UnexpectedEndOfInput(input.CurrentLocation);
                if (c == '\'')
                {
                    buffer.Add(c);
                    c = input.GetNext();
                    buffer.Add(c);
                    if (c == '\\')
                    {
                        c = input.GetNext();
                        buffer.Add(c);
                        c = input.GetNext();
                        while (c != '\'')
                        {
                            buffer.Add(c);
                            c = input.GetNext();
                        }
                    }
                    else
                        c = input.Expect('\'');
                    buffer.Add(c);
                    continue;
                }
                if (c == '"')
                {
                    buffer.Add(c);
                    while (true)
                    {
                        c = input.GetNext();
                        if (c == '\0')
                            TokenizingException.UnexpectedEndOfInput(input.CurrentLocation);
                        if (c == '"')
                            break;
                        if (c == '\\')
                        {
                            buffer.Add(c);
                            c = input.GetNext();
                        }
                        buffer.Add(c);
                    }
                    buffer.Add(c);
                    continue;
                }
                if (c == '@' && input.Peek() == '"')
                {
                    buffer.Add(c);
                    buffer.Add(input.GetNext());
                    while (true)
                    {
                        c = input.GetNext();
                        if (c == '\0')
                            TokenizingException.UnexpectedEndOfInput(input.CurrentLocation);
                        if (c == '"')
                        {
                            if (input.Peek() != '"')
                                break;
                            buffer.Add(c);
                            c = input.GetNext();
                        }

                        buffer.Add(c);
                    }
                    buffer.Add(c);
                    continue;
                }

                if (c == '{')
                {
                    braceCount++;
                    buffer.Add(c);
                    continue;
                }

                if (c == '}')
                {
                    braceCount--;
                    if (braceCount == 0)
                        break;
                    buffer.Add(c);
                    continue;
                }

                buffer.Add(c);
            }

            //_chars.Expect('}');
            return state.Success(this, Token.CSharpLiteral(new string(buffer.ToArray())), input.Consumed - startConsumed, input.CurrentLocation);
        }

        public string Name { get; set; }

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public IParser ReplaceChild(IParser find, IParser replace) => this;

        IResult IParser<char>.Parse(ParseState<char> state) => Parse(state);
    }
}

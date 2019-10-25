using System.Collections.Generic;
using System.Linq;
using Scoop.Parsing.Parsers;
using Scoop.Parsing.Sequences;

namespace Scoop.Parsing.Tokenization.Parsers
{
    public class CSharpLiteralParser : IParser<char, Token>
    {
        public IParseResult<Token> Parse(ISequence<char> _chars)
        {
            // Attempt to read through an arbitrary c# code literal. We can largely do this by 
            // counting braces, but we have to get a bit more involved when we deal with 
            // braces which are quoted as chars and strings: '{' and "{".
            var a = _chars.GetNext();
            var b = _chars.Peek();
            if (a != 'c' || b != '#')
            {
                _chars.PutBack(a);
                return Result<Token>.Fail();
            }

            _chars.GetNext();

            // TODO: Need to figure out how to properly handle unexpected end of input
            while (char.IsWhiteSpace(_chars.Peek()))
                _chars.GetNext();
            _chars.Expect('{');
            int braceCount = 1;
            var buffer = new List<char>();
            while (true)
            {
                var c = _chars.GetNext();
                //if (c == '\0')
                //    TokenizingException.UnexpectedEndOfInput();
                if (c == '\'')
                {
                    buffer.Add(c);
                    c = _chars.GetNext();
                    buffer.Add(c);
                    if (c == '\\')
                    {
                        c = _chars.GetNext();
                        buffer.Add(c);
                        c = _chars.GetNext();
                        while (c != '\'')
                        {
                            buffer.Add(c);
                            c = _chars.GetNext();
                        }
                    }
                    else
                        c = _chars.Expect('\'');
                    buffer.Add(c);
                    continue;
                }
                if (c == '"')
                {
                    buffer.Add(c);
                    while (true)
                    {
                        c = _chars.GetNext();
                        //if (c == '\0')
                        //    TokenizingException.UnexpectedEndOfInput(l);
                        if (c == '"')
                            break;
                        if (c == '\\')
                        {
                            buffer.Add(c);
                            c = _chars.GetNext();
                        }
                        buffer.Add(c);
                    }
                    buffer.Add(c);
                    continue;
                }
                if (c == '@' && _chars.Peek() == '"')
                {
                    buffer.Add(c);
                    buffer.Add(_chars.GetNext());
                    while (true)
                    {
                        c = _chars.GetNext();
                        //if (c == '\0')
                        //    TokenizingException.UnexpectedEndOfInput(l);
                        if (c == '"')
                        {
                            if (_chars.Peek() != '"')
                                break;
                            buffer.Add(c);
                            c = _chars.GetNext();
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
            return new Result<Token>(true, Token.CSharpLiteral(new string(buffer.ToArray())));
        }

        public IParseResult<object> ParseUntyped(ISequence<char> t) => Parse(t);

        public string Name { get; set; }

        public IParser Accept(IParserVisitorImplementation visitor) => this;

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public IParser ReplaceChild(IParser find, IParser replace) => this;
    }
}
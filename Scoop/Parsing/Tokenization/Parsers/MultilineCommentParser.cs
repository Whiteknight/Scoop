using System.Collections.Generic;
using System.Linq;
using ParserObjects;

namespace Scoop.Parsing.Tokenization.Parsers
{
    public class MultilineCommentParser : IParser<char, string>
    {
        public IResult<string> Parse(ParseState<char> state)
        {
            var t = state.Input;
            int startConsumed = t.Consumed;
            var a = t.GetNext();
            var b = t.Peek();
            if (a != '/' || b != '*')
            {
                t.PutBack(a);
                return state.Fail(this, "");
            }

            var chars = new List<char> { '/', t.GetNext() };
            while (true)
            {
                var c = t.GetNext();
                if (c == '\0')
                {
                    t.PutBack(c);
                    // TODO: What to do about unexpected end of input?
                    break;
                }

                if (c == '*')
                {
                    if (t.Peek() == '/')
                    {
                        t.GetNext();
                        chars.Add('*');
                        chars.Add('/');
                        break;
                    }
                }

                chars.Add(c);
            }

            var x = new string(chars.ToArray());
            return state.Success(this, x, t.Consumed - startConsumed);
        }

        public string Name { get; set; }

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public IParser ReplaceChild(IParser find, IParser replace) => this;

        IResult IParser<char>.Parse(ParseState<char> state) => Parse(state);
    }
}

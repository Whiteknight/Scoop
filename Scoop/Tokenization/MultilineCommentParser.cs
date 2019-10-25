using System.Collections.Generic;
using System.Linq;
using Scoop.Parsers;
using Scoop.Parsers.Visiting;

namespace Scoop.Tokenization
{
    public class MultilineCommentParser : IParser<char, Token>
    {
        public IParseResult<Token> Parse(ISequence<char> t)
        {
            var a = t.GetNext();
            var b = t.Peek();
            if (a != '/' || b != '*')
            {
                t.PutBack(a);
                return Result<Token>.Fail();
            }

            t.GetNext();
            var chars = new List<char> { '/', '*' };
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
            return new Result<Token>(true, Token.Comment(x));
        }

        public IParseResult<object> ParseUntyped(ISequence<char> t) => Parse(t).Untype();

        public string Name { get; set; }

        public IParser Accept(IParserVisitorImplementation visitor) => this;

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public IParser ReplaceChild(IParser find, IParser replace) => this;
    }
}
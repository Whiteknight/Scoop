using System.Collections.Generic;
using System.Linq;
using Scoop.Parsers;
using Scoop.Parsers.Visiting;

namespace Scoop.Tokenization
{
    public partial class TokenParser : IParser<char, Token>
    {
        private readonly SymbolSequence _operators;

        public TokenParser()
        {
            _operators = new SymbolSequence();

            // punctuation
            _operators.Add(".", "?.", ",", ";");

            // Parens
            _operators.Add("(", ")", "{", "}", "[", "]");

            // Arithmetic operators
            _operators.Add("+", "-", "/", "*", "&", "|", "^");

            // Logical operators
            _operators.Add("&&", "||");

            // Unary ~ and !. (Unary - and + are covered above)
            _operators.Add("~", "!");

            // prefix/postfix increment/decrement
            _operators.Add("++", "--");

            // Lambda operators
            _operators.Add("=>");

            // Coalesce operator
            _operators.Add("??");

            // Ternary/Conditional operators
            _operators.Add("?", ":");

            // Assignment operators
            _operators.Add("=", "+=", "-=", "*=", "/=", "%=", "&=", "^=", "|=");

            // Comparison operators
            _operators.Add("==", "!=", ">", "<", ">=", "<=");
        }

        public IParseResult<Token> Parse(ISequence<char> _chars)
        {
            var c = _chars.Peek();
            if (c == '\0')
                return new Result<Token>(true, Token.EndOfInput());
            if (char.IsWhiteSpace(c))
                return ReadWhitespace(_chars);
            if (c == '/')
            {
                _chars.GetNext();
                if (_chars.Peek() == '/')
                {
                    var l = _chars.CurrentLocation;
                    _chars.GetNext();
                    var x = ReadLine(_chars);
                    return new Result<Token>(true, Token.Comment(x, l));
                }
                if (_chars.Peek() == '*')
                    return ReadMultilineComment(_chars);
                    
                _chars.PutBack(c);
            }

            if (c == 'c')
            {
                c = _chars.GetNext();
                if (_chars.Peek() == '#')
                    return ReadCSharpCodeLiteral(_chars);
                _chars.PutBack(c);
                // Fall through, we might use that 'c' for an identifier somewhere
            }

            // TODO: "global" keyword and "::" operator
            // a keyword followed by an operator
            if (c == '_' || char.IsLetter(c))
                return ReadWord(_chars);

            if (char.IsNumber(c))
                return ReadNumber(_chars);
            if (c == '\'')
                return ReadCharacter(_chars);
            if (c == '@')
            {
                _chars.GetNext();
                if (char.IsLetter(_chars.Peek()))
                {
                    var word = ReadWord(_chars);
                    return new Result<Token>(true, Token.Word("@" + word.Value.Value, word.Value.Location));
                }

                _chars.PutBack(c);
            }

            if (c == '"' || c == '$' || c == '@')
                return ReadString(_chars);
            if (char.IsPunctuation(c) || char.IsSymbol(c))
                return ReadOperator(_chars);

            // Advance so the next call to ScanNext can return something new
            throw TokenizingException.UnexpectedCharacter(_chars.GetNext(), _chars.CurrentLocation);
        }

        private IParseResult<Token> ReadCSharpCodeLiteral(ISequence<char> _chars)
        {
            // Attempt to read through an arbitrary c# code literal. We can largely do this by 
            // counting braces, but we have to get a bit more involved when we deal with 
            //  braces which are quoted as chars and strings: '{' and "{".
            var l = _chars.CurrentLocation;
            _chars.Expect('#');
            while (char.IsWhiteSpace(_chars.Peek()))
                _chars.GetNext();
            _chars.Expect('{');
            int braceCount = 1;
            var buffer = new List<char>();
            while (true)
            {
                var c = _chars.GetNext();
                if (c == '\0')
                    TokenizingException.UnexpectedEndOfInput(l);
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
                        if (c == '\0')
                            TokenizingException.UnexpectedEndOfInput(l);
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
                        if (c == '\0')
                            TokenizingException.UnexpectedEndOfInput(l);
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
            return new Result<Token>(true, Token.CSharpLiteral(new string(buffer.ToArray()), l));
        }

        private static readonly HashSet<char> _hexChars = new HashSet<char>("0123456789abcdefABCDEF");

        private IParseResult<Token> ReadCharacter(ISequence<char> _chars)
        {
            _chars.Expect('\'');
            var l = _chars.CurrentLocation;
            var c = _chars.GetNext();
            if (c != '\\')
            {
                _chars.Expect('\'');
                return new Result<Token>(true, Token.Character($"'{c}'", l));
            }

            c = _chars.GetNext();
            if (c != 'x')
            {
                _chars.Expect('\'');
                return new Result<Token>(true, Token.Character($"'\\{c}'", l));
            }

            var buffer = new char[4];
            int i = 0;
            for (; i < 4; i++)
            {
                if (!_hexChars.Contains(_chars.Peek()))
                    break;
                buffer[i] = _chars.GetNext();
            }

            _chars.Expect('\'');
            return new Result<Token>(true, Token.Character($"'\\x{new string(buffer, 0, i)}'", l));
        }

        private IParseResult<Token> ReadWhitespace(ISequence<char> _chars)
        {
            var l = _chars.CurrentLocation;
            var chars = new List<char>();
            while (true)
            {
                var c = _chars.GetNext();
                if (!char.IsWhiteSpace(c))
                {
                    _chars.PutBack(c);
                    break;
                }

                chars.Add(c);
            }

            var w = new string(chars.ToArray());
            return new Result<Token>(true, Token.Whitespace(w, l));
        }

        private string ReadLine(ISequence<char> _chars)
        {
            var chars = new List<char>();
            while (true)
            {
                var c = _chars.GetNext();
                if (c == '\r' || c == '\n' || c == '\0')
                {
                    _chars.PutBack(c);
                    break;
                }

                chars.Add(c);
            }

            return new string(chars.ToArray());
        }

        private IParseResult<Token> ReadMultilineComment(ISequence<char> _chars)
        {
            var l = _chars.CurrentLocation;
            _chars.Expect('*');
            var chars = new List<char>();
            while (true)
            {
                var c = _chars.GetNext();
                if (c == '\0')
                {
                    _chars.PutBack(c);
                    break;
                }
                if (c == '*')
                {
                    if (_chars.Peek() == '/')
                    {
                        _chars.GetNext();
                        break;
                    }
                }

                chars.Add(c);
            }

            var x = new string(chars.ToArray());
            return new Result<Token>(true, Token.Comment(x, l));
        }

        private IParseResult<Token> ReadNumber(ISequence<char> _chars)
        {
            var l = _chars.CurrentLocation;
            var chars = new List<char>();
            char c;
            bool hasDecimal = false;
            while (true)
            {
                c = _chars.GetNext();
                if (!char.IsNumber(c))
                {
                    _chars.PutBack(c);
                    break;
                }

                chars.Add(c);
            }

            if (_chars.Peek() == '.')
            {
                var dot = _chars.GetNext();
                if (!char.IsDigit(_chars.Peek()))
                {
                    // The . is for method invocation, not a decimal, so we put back
                    _chars.PutBack(dot);
                    return new Result<Token>(true, new Token(new string(chars.ToArray()), TokenType.Integer, l));
                }

                hasDecimal = true;
                chars.Add(dot);
                while (true)
                {
                    c = _chars.GetNext();
                    if (!char.IsNumber(c))
                    {
                        _chars.PutBack(c);
                        break;
                    }

                    chars.Add(c);
                }
            }

            c = _chars.GetNext();
            if (c == 'F')
                return new Result<Token>(true, new Token(new string(chars.ToArray()), TokenType.Float, l));
            if (c == 'M')
                return new Result<Token>(true, new Token(new string(chars.ToArray()), TokenType.Decimal, l));
            if (c == 'L' && !hasDecimal)
                return new Result<Token>(true, new Token(new string(chars.ToArray()), TokenType.Long, l));
            if (c == 'U' && !hasDecimal)
            {
                c = _chars.Peek();
                if (c == 'L')
                {
                    _chars.GetNext();
                    return new Result<Token>(true, new Token(new string(chars.ToArray()), TokenType.ULong, l));
                }

                return new Result<Token>(true, new Token(new string(chars.ToArray()), TokenType.UInteger, l));
            }

            _chars.PutBack(c);

            if (hasDecimal)
                return new Result<Token>(true, new Token(new string(chars.ToArray()), TokenType.Double, l));
            return new Result<Token>(true, new Token(new string(chars.ToArray()), TokenType.Integer, l));
        }

        private IParseResult<Token> ReadWord(ISequence<char> _chars)
        {
            var l = _chars.CurrentLocation;
            var chars = new List<char>();
            while (true)
            {
                var c = _chars.GetNext();
                if (!char.IsLetterOrDigit(c) && c != '_')
                {
                    _chars.PutBack(c);
                    break;
                }

                chars.Add(c);
            }

            var x = new string(chars.ToArray());
            if (x == "is" || x == "as")
                return new Result<Token>(true, Token.Operator(x, l));
            return new Result<Token>(true, Token.Word(x, l));
        }

        private IParseResult<Token> ReadOperator(ISequence<char> _chars)
        {
            var l = _chars.CurrentLocation;
            var op = ReadOperator(_chars, _operators);
            if (string.IsNullOrEmpty(op))
                throw TokenizingException.UnexpectedCharacter(_chars.GetNext(), l);
            return new Result<Token>(true, Token.Operator(op, l));
        }

        private string ReadOperator(ISequence<char> _chars, SymbolSequence op)
        {
            var x = _chars.GetNext();
            if (!char.IsPunctuation(x) && !char.IsSymbol(x))
            {
                _chars.PutBack(x);
                return op.Operator;
            }
            var nextOp = op.Get(x);
            if (nextOp == null)
            {
                _chars.PutBack(x);
                return op.Operator;
            }

            return ReadOperator(_chars, nextOp);
        }

        public IParseResult<object> ParseUntyped(ISequence<char> t) => Parse(t);

        public string Name { get; set; }

        public IParser Accept(IParserVisitorImplementation visitor) => this;

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public IParser ReplaceChild(IParser find, IParser replace) => this;
    }
}
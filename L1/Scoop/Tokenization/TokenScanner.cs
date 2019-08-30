using System;
using System.Collections.Generic;

namespace Scoop.Tokenization
{
    public class TokenScanner
    {
        private readonly ICharacterSequence _chars;
        private readonly SymbolSequence _operators;

        public TokenScanner(string s)
            : this(new StringCharacterSequence(s))
        {
        }

        public TokenScanner(ICharacterSequence chars)
        {
            _chars = chars;
            _operators = new SymbolSequence();

            // punctuation
            _operators.Add(".", "?.", ",", ";");

            // Parens
            _operators.Add("(", ")", "{", "}", "[", "]");

            // Arithmetic operators
            _operators.Add("+", "-", "/", "*", "&", "|", "^");

            // Unary ~. Unary - and + are covered above
            _operators.Add("~", "!");

            // prefix/postfix increment/decrement
            _operators.Add("++", "--");

            // Lambda operators
            _operators.Add("=>");

            // Coalesce operator
            _operators.Add("??");

            // Assignment operators
            _operators.Add("=", "+=", "-=", "*=", "/=", "%=", "&=", "^=", "|=");

            // Comparison operators
            _operators.Add("==", "!=", ">", "<", ">=", "<=");
        }

        public Token ParseNext()
        {
            var c = _chars.Peek();
            if (c == '\0')
                return Token.EndOfInput();
            if (char.IsWhiteSpace(c))
                return ReadWhitespace();
            if (c == '/')
            {
                _chars.GetNext();
                if (_chars.Peek() == '/')
                {
                    var l = _chars.GetLocation();
                    _chars.GetNext();
                    var x = ReadLine();
                    return Token.Comment(x, l);
                }
                if (_chars.Peek() == '*')
                    return ReadMultilineComment();
                    
                _chars.PutBack(c);
            }

            if (c == 'c')
            {
                c = _chars.GetNext();
                if (_chars.Peek() == '#')
                    return ReadCSharpCodeLiteral();
                _chars.PutBack(c);
                // Fall through, we might use that 'c' for an identifier somewhere
            }
            // TODO: "as" and "is" are operators

            // TODO: global:: which should be treated as a single keyword for our purposes and not
            // a keyword followed by an operator
            if (c == '_' || char.IsLetter(c))
                return ReadWord();

            if (char.IsNumber(c))
                return ReadNumber();
            if (c == '\'')
                return ReadCharacter();
            if (c == '@')
            {
                // TODO: Case where we use @keyword for identifier names, otherwise fall through because it might prefix a string
            }

            if (c == '"' || c == '$' || c == '@')
                return ReadString();
            if (char.IsPunctuation(c) || char.IsSymbol(c))
                return ReadOperator();

            throw new Exception($"Unexpected character '{c}'");
        }

        private Token ReadCSharpCodeLiteral()
        {
            // Attempt to read through an arbitrary c# code literal. We can largely do this by 
            // counting braces, but we have to get a bit more involved when we deal with 
            //  braces which are quoted as chars and strings: '{' and "{".
            var l = _chars.GetLocation();
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
                    ParsingException.UnexpectedEndOfInput(l);
                if (c == '\'')
                {
                    buffer.Add(c);
                    c = _chars.GetNext();
                    if (c == '\\')
                    {
                        buffer.Add(c);
                        c = _chars.GetNext();
                        if (c == 'x')
                        {
                            // TODO: hex escape codes
                        }
                    }
                    buffer.Add(c);
                    c = _chars.Expect('\'');
                }
                if (c == '"')
                {
                    buffer.Add(c);
                    while (true)
                    {
                        c = _chars.GetNext();
                        if (c == '\0')
                            ParsingException.UnexpectedEndOfInput(l);
                        if (c == '"')
                            break;
                        if (c == '\\')
                        {
                            buffer.Add(c);
                            c = _chars.GetNext();
                        }
                        buffer.Add(c);
                    }
                }
                if (c == '@' && _chars.Peek() == '"')
                {
                    buffer.Add(c);
                    buffer.Add(_chars.GetNext());
                    while (true)
                    {
                        c = _chars.GetNext();
                        if (c == '\0')
                            ParsingException.UnexpectedEndOfInput(l);
                        if (c == '"')
                        {
                            if (_chars.Peek() != '"')
                                break;
                            buffer.Add(c);
                            c = _chars.GetNext();
                        }

                        buffer.Add(c);
                    }
                }


                if (c == '{')
                    braceCount++;
                if (c == '}')
                {
                    braceCount--;
                    if (braceCount == 0)
                        break;
                }

                buffer.Add(c);
            }

            //_chars.Expect('}');
            return Token.CSharpLiteral(new string(buffer.ToArray()), l);
        }

        private Token ReadCharacter()
        {
            char c;
            _chars.GetNext();
            var l = _chars.GetLocation();
            // TODO: Escape sequences
            c = _chars.GetNext();
            _chars.Expect('\'');
            return Token.Character(c.ToString(), l);
        }

        private Token ReadWhitespace()
        {
            var l = _chars.GetLocation();
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
            return Token.Whitespace(w, l);
        }

        private string ReadLine()
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

        private Token ReadMultilineComment()
        {
            var l = _chars.GetLocation();
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
            return Token.Comment(x, l);
        }

        private Token ReadNumber()
        {
            var l = _chars.GetLocation();
            var chars = new List<char>();
            char c;
            bool hasDecimal = false;;
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
                    return new Token(new string(chars.ToArray()), TokenType.Integer, l);
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
            {
                chars.Add(c);
                return new Token(new string(chars.ToArray()), TokenType.Float, l);
            }
            if (c == 'M')
            {
                chars.Add(c);
                return new Token(new string(chars.ToArray()), TokenType.Decimal, l);
            }
            if (c == 'L' && !hasDecimal)
            {
                chars.Add(c);
                return new Token(new string(chars.ToArray()), TokenType.Long, l);
            }
            if (c == 'U')
            {
                chars.Add(c);
                c = _chars.Peek();
                if (c == 'L')
                {
                    chars.Add(c);
                    _chars.GetNext();
                    return new Token(new string(chars.ToArray()), TokenType.ULong, l);
                }

                return new Token(new string(chars.ToArray()), TokenType.UInteger, l);
            }

            _chars.PutBack(c);

            if (hasDecimal)
                return new Token(new string(chars.ToArray()), TokenType.Double, l);
            return new Token(new string(chars.ToArray()), TokenType.Integer, l);
        }

        private Token ReadWord()
        {
            var l = _chars.GetLocation();
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
            return Token.Word(x, l);
        }

        private Token ReadString()
        {
            // TODO: Handle $ prefix, including nested {} parsing which may contain quotes or slashes or other things to trip up on
            // TODO: Handle @-style strings
            var l = _chars.GetLocation();
            var chars = new List<char>();
            _chars.Expect('"');

            while (true)
            {
                var c = _chars.GetNext();
                if (c != '"')
                {
                    chars.Add(c);
                    continue;
                }

                var n = _chars.GetNext();
                if (n == '"')
                {
                    chars.Add(c);
                    continue;
                }

                _chars.PutBack(n);
                var x = (new string(chars.ToArray()));
                return Token.String(x, l);
            }
        }

        private Token ReadOperator()
        {
            var l = _chars.GetLocation();
            var op = ReadOperator(_operators);
            return Token.Operator(op, l);
        }

        private string ReadOperator(SymbolSequence op)
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

            var recurse = ReadOperator(nextOp);
            if (recurse != null)
                return recurse;
            _chars.PutBack(x);
            if (!string.IsNullOrEmpty(op.Operator))
                return op.Operator;
            return null;
        }
    }
}
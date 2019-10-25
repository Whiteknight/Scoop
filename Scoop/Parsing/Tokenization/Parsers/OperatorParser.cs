using System.Collections.Generic;
using System.Linq;
using Scoop.Parsing.Parsers;

namespace Scoop.Parsing.Tokenization.Parsers
{
    public class OperatorParser : IParser<char, Token>
    {
        private readonly SymbolSequence _operators;

        public OperatorParser()
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

        public IParseResult<Token> Parse(ISequence<char> t)
        {
            var op = ReadOperator(t, _operators);
            if (string.IsNullOrEmpty(op))
                return new Result<Token>(false, default);
            return new Result<Token>(true, Token.Operator(op));
        }

        public IParseResult<object> ParseUntyped(ISequence<char> t) => (IParseResult<object>) Parse(t);

        public string Name { get; set; }

        public IParser Accept(IParserVisitorImplementation visitor) => this;

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public IParser ReplaceChild(IParser find, IParser replace) => this;

        private string ReadOperator(ISequence<char> t, SymbolSequence op)
        {
            var x = t.GetNext();
            if (!char.IsPunctuation(x) && !char.IsSymbol(x))
            {
                t.PutBack(x);
                return op.Operator;
            }
            var nextOp = op.Get(x);
            if (nextOp == null)
            {
                t.PutBack(x);
                return op.Operator;
            }

            return ReadOperator(t, nextOp);
        }

        private class SymbolSequence
        {
            private readonly Dictionary<char, SymbolSequence> _next;
            public string Operator { get; private set; }

            public SymbolSequence()
            {
                _next = new Dictionary<char, SymbolSequence>();
            }

            public void Add(params string[] chars)
            {
                foreach (var c in chars)
                    Add(c, 0);
            }

            private void Add(string chars, int idx)
            {
                bool isLast = idx == chars.Length;
                if (isLast)
                {
                    Operator = chars;
                    return;
                }
                var c = chars[idx];
                if (!_next.ContainsKey(c))
                    _next.Add(c, new SymbolSequence());
                _next[c].Add(chars, idx + 1);
            }

            public SymbolSequence Get(char c)
            {
                if (_next.ContainsKey(c))
                    return _next[c];
                return null;
            }
        }
    }
}
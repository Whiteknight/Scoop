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
            var op = _operators.GetOperator(t);
            if (string.IsNullOrEmpty(op))
                return new Result<Token>(false, default);
            return new Result<Token>(true, Token.Operator(op));
        }

        public IParseResult<object> ParseUntyped(ISequence<char> t) => Parse(t).Untype();

        public string Name { get; set; }

        public IParser Accept(IParserVisitorImplementation visitor) => this;

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public IParser ReplaceChild(IParser find, IParser replace) => this;

        // Operator Trie type to get operator sequences
        // This might be overkill, because no operators we're trying to get are more than 2 symbols long
        private class SymbolSequence
        {
            private readonly Dictionary<char, SymbolSequence> _next;
            private string _operator;

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
                    _operator = chars;
                    return;
                }
                var c = chars[idx];
                if (!_next.ContainsKey(c))
                    _next.Add(c, new SymbolSequence());
                _next[c].Add(chars, idx + 1);
            }

            public string GetOperator(ISequence<char> t)
            {
                var x = t.GetNext();
                if (!_next.ContainsKey(x))
                {
                    t.PutBack(x);
                    return _operator;
                }

                return _next[x].GetOperator(t) ?? _operator;
            }
        }
    }
}
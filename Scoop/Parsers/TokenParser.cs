using System;
using System.Collections.Generic;
using System.Linq;
using Scoop.Parsers.Visiting;
using Scoop.Tokenization;

namespace Scoop.Parsers
{
    /// <summary>
    /// Consumes a single Token of a specified type and produces an output
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    public class TokenParser<TOutput> : IParser<Token, TOutput>
    {
        private readonly TokenType _type;
        private readonly Func<Token, TOutput> _produce;

        public TokenParser(TokenType type, Func<Token, TOutput> produce)
        {
            _type = type;
            _produce = produce;
        }

        public IParseResult<TOutput> Parse(ISequence<Token> t)
        {
            if (t.Peek().IsType(_type))
                return new Result<TOutput>(true, _produce(t.GetNext()));
            return Result<TOutput>.Fail();
        }

        IParseResult<object> IParser<Token>.ParseUntyped(ISequence<Token> t) => (IParseResult<object>)Parse(t);

        public string Name { get; set; }

        public IParser Accept(IParserVisitorImplementation visitor) => visitor.VisitToken(this);

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public IParser ReplaceChild(IParser find, IParser replace) => this;

        public override string ToString()
        {
            var typeName = this.GetType().Name;
            return Name == null ? base.ToString() : $"{typeName} {Name}";
        }
    }
}

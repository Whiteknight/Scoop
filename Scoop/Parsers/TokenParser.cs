using System;
using System.Collections.Generic;
using System.Linq;
using Scoop.Parsers.Visiting;
using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop.Parsers
{
    /// <summary>
    /// Consumes a single Token of a specified type and produces an output
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    public class TokenParser<TOutput> : IParser<TOutput>
        where TOutput : AstNode
    {
        private readonly TokenType _type;
        private readonly Func<Token, TOutput> _produce;

        public TokenParser(TokenType type, Func<Token, TOutput> produce)
        {
            _type = type;
            _produce = produce;
        }

        public TOutput Parse(ITokenizer t)
        {
            if (t.Peek().IsType(_type))
                return _produce(t.GetNext());
            return null;
        }

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

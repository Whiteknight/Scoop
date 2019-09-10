using System;
using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop.Parsers
{
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

        public TOutput TryParse(ITokenizer t)
        {
            if (t.Peek().IsType(_type))
                return _produce(t.GetNext());
            return null as TOutput;
        }

        public string Name { get; set; }
        public override string ToString()
        {
            var typeName = this.GetType().Name;
            return Name == null ? base.ToString() : $"{typeName} {Name}";
        }
    }
}

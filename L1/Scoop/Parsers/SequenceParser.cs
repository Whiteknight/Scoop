using System;
using System.Collections.Generic;
using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop.Parsers
{

    public class SequenceParser<TOutput> : IParser<TOutput>
    {
        private readonly IReadOnlyList<IParser<AstNode>> _parsers;
        private readonly Func<IReadOnlyList<AstNode>, TOutput> _produce;

        public SequenceParser(IReadOnlyList<IParser<AstNode>> parsers, Func<IReadOnlyList<AstNode>, TOutput> produce)
        {
            _parsers = parsers;
            _produce = produce;
        }

        public TOutput TryParse(ITokenizer t)
        {
            var outputs = new AstNode[_parsers.Count];
            for (int i = 0; i < _parsers.Count; i++)
            {
                var result = _parsers[i].Parse(t);
                outputs[i] = result.GetResult();
            }
            return _produce(outputs);
        }

        public string Name { get; set; }
        public override string ToString()
        {
            var typeName = this.GetType().Name;
            return Name == null ? base.ToString() : $"{typeName} {Name}";
        }
    }
}

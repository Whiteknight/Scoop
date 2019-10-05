using System;
using System.Collections.Generic;
using System.Linq;
using Scoop.Parsers.Visiting;
using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop.Parsers
{
    /// <summary>
    /// Parses a list of steps and produces a single output
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    public class SequenceParser<TOutput> : IParser<TOutput>
    {
        private readonly IReadOnlyList<IParser<AstNode>> _parsers;
        private readonly Func<IReadOnlyList<AstNode>, TOutput> _produce;

        public SequenceParser(IReadOnlyList<IParser<AstNode>> parsers, Func<IReadOnlyList<AstNode>, TOutput> produce)
        {
            _parsers = parsers;
            _produce = produce;
        }

        public TOutput Parse(ITokenizer t)
        {
            t = t.Mark();
            var outputs = new AstNode[_parsers.Count];
            for (int i = 0; i < _parsers.Count; i++)
            {
                var result = _parsers[i].Parse(t);
                if (result == null)
                {
                    (t as WindowTokenizer)?.Rewind();
                    return default;
                }

                outputs[i] = result;
            }
            return _produce(outputs);
        }

        public string Name { get; set; }

        public IParser Accept(IParserVisitorImplementation visitor) => visitor.VisitSequence(this);

        public IEnumerable<IParser> GetChildren() => _parsers;

        public IParser ReplaceChild(IParser find, IParser replace)
        {
            if (_parsers.Contains(find) && replace is IParser<AstNode> realReplace)
            {
                var newList = new IParser<AstNode>[_parsers.Count];
                for (int i = 0; i < _parsers.Count; i++)
                {
                    var child = _parsers[i];
                    newList[i] = child == find ? realReplace : child;
                }

                return new SequenceParser<TOutput>(newList, _produce);
            }

            return this;
        }

        public override string ToString()
        {
            var typeName = this.GetType().Name;
            return Name == null ? base.ToString() : $"{typeName} {Name}";
        }
    }
}

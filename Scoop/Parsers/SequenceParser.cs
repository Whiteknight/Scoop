using System;
using System.Collections.Generic;
using System.Linq;
using Scoop.Parsers.Visiting;
using Scoop.Tokenization;

namespace Scoop.Parsers
{
    /// <summary>
    /// Parses a list of steps and produces a single output
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <typeparam name="TInput"></typeparam>
    public class SequenceParser<TInput, TOutput> : IParser<TInput, TOutput>
    {
        private readonly IReadOnlyList<IParser<TInput>> _parsers;
        private readonly Func<IReadOnlyList<object>, TOutput> _produce;

        public SequenceParser(IReadOnlyList<IParser<TInput>> parsers, Func<IReadOnlyList<object>, TOutput> produce)
        {
            _parsers = parsers;
            _produce = produce;
        }

        public IParseResult<TOutput> Parse(ISequence<TInput> t)
        {
            t = t.Mark();
            var outputs = new object[_parsers.Count];
            for (int i = 0; i < _parsers.Count; i++)
            {
                var result = _parsers[i].ParseUntyped(t);
                if (!result.Success)
                {
                    (t as WindowTokenizer<TInput>)?.Rewind();
                    return Result<TOutput>.Fail();
                }

                outputs[i] = result.Value;
            }
            return new Result<TOutput>(true, _produce(outputs));
        }

        IParseResult<object> IParser<TInput>.ParseUntyped(ISequence<TInput> t) => (IParseResult<object>) Parse(t);

        public string Name { get; set; }

        public IParser Accept(IParserVisitorImplementation visitor) => visitor.VisitSequence(this);

        public IEnumerable<IParser> GetChildren() => _parsers;

        public IParser ReplaceChild(IParser find, IParser replace)
        {
            if (_parsers.Contains(find) && replace is IParser<TInput> realReplace)
            {
                var newList = new IParser<TInput>[_parsers.Count];
                for (int i = 0; i < _parsers.Count; i++)
                {
                    var child = _parsers[i];
                    newList[i] = child == find ? realReplace : child;
                }

                return new SequenceParser<TInput, TOutput>(newList, _produce);
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

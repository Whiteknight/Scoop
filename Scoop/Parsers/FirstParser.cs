using System.Collections.Generic;
using System.Linq;
using Scoop.Parsers.Visiting;
using Scoop.Tokenization;

namespace Scoop.Parsers
{
    /// <summary>
    /// Takes a list of parsers and attempts each one in order. Returns as soon as the first parser
    /// succeeds
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <typeparam name="TInput"></typeparam>
    public class FirstParser<TInput, TOutput> : IParser<TInput, TOutput>
    {
        private readonly IReadOnlyList<IParser<TInput, TOutput>> _parsers;

        public FirstParser(params IParser<TInput, TOutput>[] parsers)
        {
            _parsers = parsers;
        }

        public IParseResult<TOutput> Parse(ISequence<TInput> t)
        {
            for (int i = 0; i < _parsers.Count; i++)
            {
                var parser = _parsers[i];
                var result = parser.Parse(t);
                if (result.Success)
                    return result;
            }

            return Result<TOutput>.Fail();
        }

        IParseResult<object> IParser<TInput>.ParseUntyped(ISequence<TInput> t) => (IParseResult<object>)Parse(t);

        public string Name { get; set; }

        public IParser Accept(IParserVisitorImplementation visitor) => visitor.VisitFirst(this);

        public IEnumerable<IParser> GetChildren() => _parsers;

        public IParser ReplaceChild(IParser find, IParser replace)
        {
            if (_parsers.Contains(find) && replace is IParser<TInput, TOutput> realReplace)
            {
                var newList = new IParser<TInput, TOutput>[_parsers.Count];
                for (int i = 0; i < _parsers.Count; i++)
                {
                    var child = _parsers[i];
                    newList[i] = child == find ? realReplace : child;
                }

                return new FirstParser<TInput, TOutput>(newList);
            }

            return this;
        }

        public override string ToString()
        {
            var typeName = GetType().Name;
            return Name == null ? base.ToString() : $"{typeName} {Name}";
        }
    }
}

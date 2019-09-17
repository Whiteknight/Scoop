using System;
using Scoop.Tokenization;

namespace Scoop.Parsers
{
    /// <summary>
    /// Parser to produce an output node unconditionally. Consumes no input.
    /// This is used to provide a default node value
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    public class ProduceParser<TOutput> : IParser<TOutput>
    {
        private readonly Func<TOutput> _produce;

        public ProduceParser(Func<TOutput> produce)
        {
            _produce = produce;
        }

        public TOutput TryParse(ITokenizer t)
        {
            return _produce();
        }

        public string Name { get; set; }
        public override string ToString()
        {
            var typeName = this.GetType().Name;
            return Name == null ? base.ToString() : $"{typeName} {Name}";
        }
    }
}

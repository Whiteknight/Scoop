using System;
using Scoop.Tokenization;

namespace Scoop.Parsers
{
    /// <summary>
    /// Adaptor object to convert from an old-style Recursive Descent parsing method to
    /// IParser
    /// This type will disappear when all old-style parsing methods have been updated
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    public class OldStyleRuleParser<TOutput> : IParser<TOutput>
    {
        private readonly Func<ITokenizer, TOutput> _parse;

        public OldStyleRuleParser(Func<ITokenizer, TOutput> parse)
        {
            _parse = parse;
        }

        public TOutput TryParse(ITokenizer t) => _parse(t);

        public string Name { get; set; }
        public override string ToString()
        {
            var typeName = this.GetType().Name;
            return Name == null ? base.ToString() : $"{typeName} {Name}";
        }
    }
}

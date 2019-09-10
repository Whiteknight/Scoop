using System;
using Scoop.Tokenization;

namespace Scoop.Parsers
{

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

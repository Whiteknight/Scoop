using System;
using Scoop.Tokenization;

namespace Scoop.Parsers
{
    /// <summary>
    /// Looks up a parser at Parse() time, to avoid circular references in the grammar
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    public class DeferredParser<TOutput> : IParser<TOutput>
    {
        private readonly Func<IParser<TOutput>> _getParser;

        public DeferredParser(Func<IParser<TOutput>> getParser )
        {
            _getParser = getParser;
        }

        public TOutput TryParse(ITokenizer t) => _getParser().TryParse(t);

        private string _name;

        public string Name
        {
            get { return _name ?? _getParser().Name; }
            set { _name = value; }
        }

        public override string ToString()
        {
            var typeName = this.GetType().Name;
            return Name == null ? base.ToString() : $"{typeName} {Name}";
        }
    }
}
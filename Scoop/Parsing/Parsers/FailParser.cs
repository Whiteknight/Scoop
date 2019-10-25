using System.Collections.Generic;
using System.Linq;

namespace Scoop.Parsing.Parsers
{
    public class FailParser<TInput, TOutput> : IParser<TInput, TOutput>
    {
        public IParseResult<TOutput> Parse(ISequence<TInput> t) => Result<TOutput>.Fail();

        IParseResult<object> IParser<TInput>.ParseUntyped(ISequence<TInput> t) => Parse(t).Untype();

        public string Name { get; set; }

        public IParser Accept(IParserVisitorImplementation visitor) => this;

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public IParser ReplaceChild(IParser find, IParser replace) => this;

        public override string ToString()
        {
            var typeName = this.GetType().Name;
            return Name == null ? base.ToString() : $"{typeName} {Name}";
        }
    }
}
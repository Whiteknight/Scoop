using System;
using System.Collections.Generic;
using System.Linq;
using Scoop.Parsers.Visiting;
using Scoop.Tokenization;

namespace Scoop.Parsers
{
    public class PredicateParser<TInput, TOutput> : IParser<TInput, TOutput>
    {
        private readonly Func<TInput, bool> _predicate;
        private readonly Func<TInput, TOutput> _produce;

        public PredicateParser(Func<TInput, bool> predicate, Func<TInput, TOutput> produce)
        {
            _predicate = predicate;
            _produce = produce;
        }

        public IParseResult<TOutput> Parse(ISequence<TInput> t)
        {
            var next = t.Peek();
            if (!_predicate(next))
                return Result<TOutput>.Fail();
            return new Result<TOutput>(true, _produce(t.GetNext()));
        }

        public IParseResult<object> ParseUntyped(ISequence<TInput> t) => (IParseResult<object>)Parse(t);

        public string Name { get; set; }

        public IParser Accept(IParserVisitorImplementation visitor) => visitor.VisitPredicate(this);

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public IParser ReplaceChild(IParser find, IParser replace) => this;

        public override string ToString()
        {
            var typeName = this.GetType().Name;
            return Name == null ? base.ToString() : $"{typeName} {Name}";
        }
    }
}

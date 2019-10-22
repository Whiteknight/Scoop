using System;
using System.Collections.Generic;
using Scoop.Parsers.Visiting;
using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop.Parsers
{
    /// <summary>
    /// Parses a list of items separated by some separator. Returns a ListNode of
    /// the results
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <typeparam name="TItem"></typeparam>
    /// <typeparam name="TSeparator"></typeparam>
    /// <typeparam name="TInput"></typeparam>
    public class SeparatedListParser<TInput, TItem, TSeparator, TOutput> : IParser<TInput, TOutput>
        where TOutput : AstNode
    {
        private readonly IParser<TInput, TItem> _itemParser;
        private readonly IParser<TInput, TSeparator> _separatorParser;
        private readonly Func<IReadOnlyList<TItem>, TOutput> _produce;
        private readonly bool _atLeastOne;

        public SeparatedListParser(IParser<TInput, TItem> itemParser, IParser<TInput, TSeparator> separatorParser, Func<IReadOnlyList<TItem>, TOutput> produce, bool atLeastOne)
        {
            _itemParser = itemParser;
            _separatorParser = separatorParser;
            _produce = produce;
            _atLeastOne = atLeastOne;
        }

        public IParseResult<TOutput> Parse(ISequence<TInput> t)
        {
            var items = new List<TItem>();
            var result = _itemParser.Parse(t);
            if (!result.Success)
                return _atLeastOne ? Result<TOutput>.Fail() : new Result<TOutput>(true, _produce(items));
            items.Add(result.Value);

            while (true)
            {
                var sepResult = _separatorParser.Parse(t);
                if (!sepResult.Success)
                    break;
                result = _itemParser.Parse(t);
                if (!result.Success)
                {
                    // TODO: This is a little bit ugly. If we can find a better way to report the error we can remove the AstNode constraint
                    var location = (sepResult.Value as AstNode)?.Location ?? (sepResult.Value as Token)?.Location;
                    return new Result<TOutput>(true, _produce(items).WithDiagnostics(location, $"Incomplete list. Expected to parse {typeof(TItem).Name} after separator but was not found"));
                }

                items.Add(result.Value);
            }
            return new Result<TOutput>(true, _produce(items));
        }

        IParseResult<object> IParser<TInput>.ParseUntyped(ISequence<TInput> t) => (IParseResult<object>)Parse(t);

        public string Name { get; set; }

        public IParser Accept(IParserVisitorImplementation visitor) => visitor.VisitSeparatedList(this);

        public IEnumerable<IParser> GetChildren() => new IParser[] { _itemParser, _separatorParser };

        public IParser ReplaceChild(IParser find, IParser replace)
        {
            if (find == _itemParser && replace is IParser<TInput, TItem> itemReplace)
                return new SeparatedListParser<TInput, TItem, TSeparator, TOutput>(itemReplace, _separatorParser, _produce, _atLeastOne);
            if (find == _separatorParser && replace is IParser<TInput, TSeparator> separatorReplace)
                return new SeparatedListParser<TInput, TItem, TSeparator, TOutput>(_itemParser, separatorReplace, _produce, _atLeastOne);
            return this;
        }

        public override string ToString()
        {
            var typeName = this.GetType().Name;
            return Name == null ? base.ToString() : $"{typeName} {Name}";
        }
    }
}
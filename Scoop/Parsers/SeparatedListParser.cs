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
    public class SeparatedListParser<TOutput, TItem> : IParser<ListNode<TOutput>>
        where TOutput : AstNode
        where TItem : AstNode
    {
        private readonly IParser<TItem> _itemParser;
        private readonly IParser<AstNode> _separatorParser;
        private readonly Func<IReadOnlyList<TItem>, ListNode<TOutput>> _produce;
        private readonly bool _atLeastOne;

        public SeparatedListParser(IParser<TItem> itemParser, IParser<AstNode> separatorParser, Func<IReadOnlyList<TItem>, ListNode<TOutput>> produce, bool atLeastOne)
        {
            _itemParser = itemParser;
            _separatorParser = separatorParser;
            _produce = produce;
            _atLeastOne = atLeastOne;
        }

        public ListNode<TOutput> TryParse(ITokenizer t)
        {
            var items = new List<TItem>();
            var result = _itemParser.TryParse(t);
            if (result == null)
                return _atLeastOne ? null : _produce(items);
            items.Add(result);

            while (_separatorParser.TryParse(t) != null)
            {
                result = _itemParser.TryParse(t);
                if (result == null)
                    return null;
                items.Add(result);
            }
            return _produce(items);
        }

        public string Name { get; set; }

        public IParser Accept(IParserVisitorImplementation visitor) => visitor.VisitSeparatedList(this);

        public IEnumerable<IParser> GetChildren() => new[] { _itemParser, _separatorParser };

        public IParser ReplaceChild(IParser find, IParser replace)
        {
            if (find == _itemParser && replace is IParser<TItem> itemReplace)
                return new SeparatedListParser<TOutput, TItem>(itemReplace, _separatorParser, _produce, _atLeastOne);
            if (find == _separatorParser && replace is IParser<AstNode> separatorReplace)
                return new SeparatedListParser<TOutput, TItem>(_itemParser, separatorReplace, _produce, _atLeastOne);
            return this;
        }

        public override string ToString()
        {
            var typeName = this.GetType().Name;
            return Name == null ? base.ToString() : $"{typeName} {Name}";
        }
    }
}
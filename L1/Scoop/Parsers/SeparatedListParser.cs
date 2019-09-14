using System;
using System.Collections.Generic;
using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop.Parsers
{
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
        public override string ToString()
        {
            var typeName = this.GetType().Name;
            return Name == null ? base.ToString() : $"{typeName} {Name}";
        }
    }
}
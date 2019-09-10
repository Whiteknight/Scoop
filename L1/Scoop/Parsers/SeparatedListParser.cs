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

        public SeparatedListParser(IParser<TItem> itemParser, IParser<AstNode> separatorParser, Func<IReadOnlyList<TItem>, ListNode<TOutput>> produce)
        {
            _itemParser = itemParser;
            _separatorParser = separatorParser;
            _produce = produce;
        }

        public ListNode<TOutput> TryParse(ITokenizer t)
        {
            var items = new List<TItem>();
            var result = _itemParser.Parse(t);
            if (!result.IsSuccess)
                return _produce(items);
            items.Add(result.Value);

            while (_separatorParser.Parse(t).IsSuccess)
            {
                result = _itemParser.Parse(t);
                items.Add(result.Value);
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
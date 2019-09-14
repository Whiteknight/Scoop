using System;
using System.Collections.Generic;
using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop.Parsers
{
    public class ListParser<TOutput, TItem> : IParser<ListNode<TOutput>>
        where TOutput : AstNode
    {
        private readonly IParser<TItem> _parser;
        private readonly Func<IReadOnlyList<TItem>, ListNode<TOutput>> _produce;

        public ListParser(IParser<TItem> parser, Func<IReadOnlyList<TItem>, ListNode<TOutput>> produce)
        {
            _parser = parser;
            _produce = produce;
        }

        public ListNode<TOutput> TryParse(ITokenizer t)
        {
            var items = new List<TItem>();
            while (true)
            {
                var result = _parser.TryParse(t);
                if (result == null)
                    break;
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

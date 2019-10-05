using System;
using System.Collections.Generic;
using Scoop.Parsers.Visiting;
using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop.Parsers
{
    /// <summary>
    /// Parses a list of productions with no explicit separator. Continues as long as the parser
    /// succeeds. Terminates and returns a ListNode when the parser fails. May return 0 items.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <typeparam name="TItem"></typeparam>
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

        public ListNode<TOutput> Parse(ITokenizer t)
        {
            var items = new List<TItem>();
            while (true)
            {
                var result = _parser.Parse(t);
                if (result == null)
                    break;
                items.Add(result);
            }
            return _produce(items);
        }

        public string Name { get; set; }

        public IParser Accept(IParserVisitorImplementation visitor) => visitor.VisitList(this);

        public IEnumerable<IParser> GetChildren() => new[] { _parser };

        public IParser ReplaceChild(IParser find, IParser replace)
        {
            if (_parser == find && replace is IParser<TItem> realReplace)
                return new ListParser<TOutput, TItem>(realReplace, _produce);
            return this;
        }

        public override string ToString()
        {
            var typeName = this.GetType().Name;
            return Name == null ? base.ToString() : $"{typeName} {Name}";
        }
    }
}

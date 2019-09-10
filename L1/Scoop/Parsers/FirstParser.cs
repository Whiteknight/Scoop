using System.Collections.Generic;
using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop.Parsers
{
    public class FirstParser<TOutput> : IParser<TOutput>
        where TOutput : AstNode
    {
        private readonly IReadOnlyList<IParser<AstNode>> _parsers;

        public FirstParser(params IParser<AstNode>[] parsers)
        {
            _parsers = parsers;
        }

        public TOutput TryParse(ITokenizer t)
        {
            for (int i = 0; i < _parsers.Count; i++)
            {
                var result = _parsers[i].Parse(t);
                if (result.IsSuccess)
                    return result.Value as TOutput;
            }
            throw ParsingException.CouldNotParseRule(nameof(FirstParser<TOutput>), t.Peek());
        }

        public string Name { get; set; }
        public override string ToString()
        {
            var typeName = this.GetType().Name;
            return Name == null ? base.ToString() : $"{typeName} {Name}";
        }
    }
}

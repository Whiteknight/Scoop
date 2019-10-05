using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Scoop.Parsers.Visiting;
using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop.Parsers
{
    /// <summary>
    /// Takes a list of parsers and attempts each one in order. Returns as soon as the first parser
    /// succeeds
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    public class FirstParser<TOutput> : IParser<TOutput>
        where TOutput : AstNode
    {
        private readonly IReadOnlyList<IParser<AstNode>> _parsers;

        public FirstParser(params IParser<AstNode>[] parsers)
        {
            _parsers = parsers;
        }

        public TOutput Parse(ITokenizer t)
        {
            for (int i = 0; i < _parsers.Count; i++)
            {
                var parser = _parsers[i];
                var result = parser.Parse(t);
                if (result != null)
                    return result as TOutput;
            }

            return null;
        }

        public string Name { get; set; }

        public IParser Accept(IParserVisitorImplementation visitor) => visitor.VisitFirst(this);

        public IEnumerable<IParser> GetChildren() => _parsers;

        public IParser ReplaceChild(IParser find, IParser replace)
        {
            if (_parsers.Contains(find) && replace is IParser<AstNode> realReplace)
            {
                var newList = new IParser<AstNode>[_parsers.Count];
                for (int i = 0; i < _parsers.Count; i++)
                {
                    var child = _parsers[i];
                    newList[i] = child == find ? realReplace : child;
                }

                return new FirstParser<TOutput>(newList);
            }

            return this;
        }

        public override string ToString()
        {
            var typeName = this.GetType().Name;
            return Name == null ? base.ToString() : $"{typeName} {Name}";
        }
    }
}

using System;

namespace Scoop.Parsers.Visiting
{
    public class ReplaceParserVisitor : IParserVisitor
    {
        private readonly Func<IParser, bool> _predicate;
        private readonly IParser _replacement;

        public ReplaceParserVisitor(Func<IParser, bool> predicate, IParser replacement)
        {
            _predicate = predicate;
            _replacement = replacement;
        }

        public IParser Visit(IParser parser)
        {
            if (_predicate(parser))
                return _replacement;

            foreach (var child in parser.GetChildren())
            {
                var newChild = Visit(child);
                if (newChild != child)
                    parser = parser.ReplaceChild(child, newChild);
            }

            return parser;
        }
    }
}
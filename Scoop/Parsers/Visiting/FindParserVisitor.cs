using System;
using System.Collections.Generic;
using System.Linq;

namespace Scoop.Parsers.Visiting
{
    public class FindParserVisitor : IParserVisitor
    {
        private readonly Func<IParser, bool> _predicate;
        private readonly bool _justOne;
        private readonly List<IParser> _found;
        private bool _canStop;

        private FindParserVisitor(List<IParser> found, Func<IParser, bool> predicate, bool justOne)
        {
            _predicate = predicate;
            _justOne = justOne;
            _canStop = false;
            _found = found;
        }

        public static IParser Named(string name, IParser root)
        {
            var found = new List<IParser>();
            new FindParserVisitor(found, p => p.Name == name, true).Visit(root);
            return found.FirstOrDefault();
        }

        public static IReadOnlyList<TParser> OfType<TParser>(IParser root)
            where TParser : IParser
        {
            var found = new List<IParser>();
            new FindParserVisitor(found, p => p is TParser, false).Visit(root);
            return found.Cast<TParser>().ToList();
        }

        public IParser Visit(IParser parser)
        {
            if (_canStop)
                return parser;

            if (_predicate(parser))
            {
                _found.Add(parser);
                if (_justOne)
                {
                    _canStop = true;
                    return parser;
                }
            }

            foreach (var child in parser.GetChildren())
            {
                Visit(child);
                if (_canStop)
                    break;
            }

            return parser;
        }
    }
}
﻿using System.Collections.Generic;
using System.Linq;
using Scoop.Parsers.Visiting;
using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop.Parsers
{
    /// <summary>
    /// Parses one of a list of allowable keywords
    /// </summary>
    public class KeywordParser : IParser<KeywordNode>
    {
        private readonly string[] _keywords;

        public KeywordParser(params string[] keywords)
        {
            _keywords = keywords;
        }

        public KeywordNode TryParse(ITokenizer t)
        {
            if (!t.Peek().IsKeyword(_keywords))
                return null;
            return new KeywordNode(t.GetNext());
        }

        public string Name { get; set; }

        public IParser Accept(IParserVisitorImplementation visitor) => visitor.VisitKeyword(this);

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public IParser ReplaceChild(IParser find, IParser replace) => this;

        public override string ToString()
        {
            var typeName = this.GetType().Name;
            return Name == null ? base.ToString() : $"{typeName} {Name}";
        }
    }
}

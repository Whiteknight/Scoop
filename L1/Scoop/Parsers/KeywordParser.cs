using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop.Parsers
{
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
        public override string ToString()
        {
            var typeName = this.GetType().Name;
            return Name == null ? base.ToString() : $"{typeName} {Name}";
        }
    }
}

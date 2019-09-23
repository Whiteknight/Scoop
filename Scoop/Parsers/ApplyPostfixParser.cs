using System;
using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop.Parsers
{
    public class ApplyPostfixParser : IParser<AstNode>
    {
        private readonly IParser<AstNode> _initial;
        private readonly Func<IParser<AstNode>, IParser<AstNode>> _produce;

        public ApplyPostfixParser(IParser<AstNode> initial, Func<IParser<AstNode>, IParser<AstNode>> produce)
        {
            _initial = initial;
            _produce = produce;
        }

        public AstNode TryParse(ITokenizer t)
        {
            var current = _initial.TryParse(t);
            if (current == null)
                return null;
            var left = new LeftParser(current);
            while (true)
            {
                var right = _produce(left);
                var rhs = right.TryParse(t);
                if (rhs == null)
                    return current;

                current = rhs;
                left = new LeftParser(current);
            }
        }

        public string Name { get; set; }

        public override string ToString()
        {
            var typeName = this.GetType().Name;
            return Name == null ? base.ToString() : $"{typeName} {Name}";
        }

        private class LeftParser : IParser<AstNode>
        {
            private readonly AstNode _left;

            public LeftParser(AstNode left)
            {
                _left = left;
            }

            public AstNode TryParse(ITokenizer t)
            {
                return _left;
            }

            public string Name { get; set; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Scoop.Parsers.Visiting;
using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop.Parsers
{
    public class ApplyPostfixParser : IParser<AstNode>
    {
        private readonly IParser<AstNode> _initial;
        private readonly Func<IParser<AstNode>, IParser<AstNode>> _getRight;

        public ApplyPostfixParser(IParser<AstNode> initial, Func<IParser<AstNode>, IParser<AstNode>> getRight)
        {
            _initial = initial;
            _getRight = getRight;
        }

        public AstNode TryParse(ITokenizer t)
        {
            var current = _initial.TryParse(t);
            if (current == null)
                return null;
            var left = new LeftParser(current);
            while (true)
            {
                var right = _getRight(left);
                var rhs = right.TryParse(t);
                if (rhs == null)
                    return current;

                current = rhs;
                left = new LeftParser(current);
            }
        }

        public string Name { get; set; }

        public IParser Accept(IParserVisitorImplementation visitor) => visitor.VisitApplyPostfix(this);

        public IEnumerable<IParser> GetChildren()
        {
            var left = new LeftParser(null);
            var right = _getRight(left);
            return new IParser[] { _initial, right };
        }

        public IParser ReplaceChild(IParser find, IParser replace)
        {
            if (_initial == find)
                return new ApplyPostfixParser(find as IParser<AstNode>, _getRight);

            // TODO: We need to be able to replace the right as well?
            return this;
        }

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
            public IParser Accept(IParserVisitor visitor) => this;

            public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

            public IParser ReplaceChild(IParser find, IParser replace) => this;
        }
    }
}

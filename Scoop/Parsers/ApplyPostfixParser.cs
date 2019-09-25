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
        private readonly IParser<AstNode> _right;
        private readonly LeftParser _left;

        public ApplyPostfixParser(IParser<AstNode> initial, Func<IParser<AstNode>, IParser<AstNode>> getRight)
        {
            _initial = initial;
            _left = new LeftParser();
            _right = getRight(_left);
        }

        private ApplyPostfixParser(IParser<AstNode> initial, LeftParser left, IParser<AstNode> right)
        {
            _initial = initial;
            _left = left;
            _right = right;
        }

        public AstNode TryParse(ITokenizer t)
        {
            var current = _initial.TryParse(t);
            if (current == null)
                return null;

            _left.Value = current;
            while (true)
            {
                var rhs = _right.TryParse(t);
                if (rhs == null)
                    return current;

                current = rhs;
                _left.Value = current;
            }
        }

        public string Name { get; set; }

        public IParser Accept(IParserVisitorImplementation visitor) => visitor.VisitApplyPostfix(this);

        public IEnumerable<IParser> GetChildren() => new IParser[] { _initial, _right };

        public IParser ReplaceChild(IParser find, IParser replace)
        {
            if (_initial == find)
                return new ApplyPostfixParser(replace as IParser<AstNode>, _left, _right);
            if (_right == find)
                return new ApplyPostfixParser(_initial, _left, replace as IParser<AstNode>);

            return this;
        }

        public override string ToString()
        {
            var typeName = this.GetType().Name;
            return Name == null ? base.ToString() : $"{typeName} {Name}";
        }

        private class LeftParser : IParser<AstNode>
        {
            public AstNode Value;

            public AstNode TryParse(ITokenizer t)
            {
                return Value;
            }

            public string Name { get; set; }

            public IParser Accept(IParserVisitorImplementation visitor) => this;

            public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

            public IParser ReplaceChild(IParser find, IParser replace) => this;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Scoop.Parsers.Visiting;
using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop.Parsers
{
    public class ApplyPostfixParser : IParser<Token, AstNode>
    {
        private readonly IParser<Token, AstNode> _initial;
        private readonly IParser<Token, AstNode> _right;
        private readonly LeftParser _left;

        public ApplyPostfixParser(IParser<Token, AstNode> initial, Func<IParser<Token, AstNode>, IParser<Token, AstNode>> getRight)
        {
            _initial = initial;
            _left = new LeftParser();
            _right = getRight(_left);
        }

        private ApplyPostfixParser(IParser<Token, AstNode> initial, LeftParser left, IParser<Token, AstNode> right)
        {
            _initial = initial;
            _left = left;
            _right = right;
        }

        public IParseResult<AstNode> Parse(ISequence<Token> t)
        {
            var result = _initial.Parse(t);
            if (!result.Success)
                return Result<AstNode>.Fail();

            var current = result.Value;
            _left.Value = result.Value;
            while (true)
            {
                var rhsResult = _right.Parse(t);
                if (!rhsResult.Success)
                    return new Result<AstNode>(true, current);

                current = rhsResult.Value;
                _left.Value = current;
            }
        }

        IParseResult<object> IParser<Token>.ParseUntyped(ISequence<Token> t) => (IParseResult<object>)Parse(t);

        public string Name { get; set; }

        public IParser Accept(IParserVisitorImplementation visitor) => visitor.VisitApplyPostfix(this);

        public IEnumerable<IParser> GetChildren() => new IParser[] { _initial, _right };

        public IParser ReplaceChild(IParser find, IParser replace)
        {
            if (_initial == find)
                return new ApplyPostfixParser(replace as IParser<Token, AstNode>, _left, _right);
            if (_right == find)
                return new ApplyPostfixParser(_initial, _left, replace as IParser<Token, AstNode>);

            return this;
        }

        public override string ToString()
        {
            var typeName = this.GetType().Name;
            return Name == null ? base.ToString() : $"{typeName} {Name}";
        }

        private class LeftParser : IParser<Token, AstNode>
        {
            public AstNode Value { get; set; }

            public IParseResult<AstNode> Parse(ISequence<Token> t) => new Result<AstNode>(true, Value);

            IParseResult<object> IParser<Token>.ParseUntyped(ISequence<Token> t) => (IParseResult<object>)Parse(t);

            public string Name { get; set; }

            public IParser Accept(IParserVisitorImplementation visitor) => this;

            public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

            public IParser ReplaceChild(IParser find, IParser replace) => this;
        }
    }
}

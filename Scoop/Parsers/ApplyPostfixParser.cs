using System;
using System.Collections.Generic;
using System.Linq;
using Scoop.Parsers.Visiting;
using Scoop.Tokenization;

namespace Scoop.Parsers
{
    public class ApplyPostfixParser<TInput, TOutput> : IParser<TInput, TOutput>
    {
        private readonly IParser<TInput, TOutput> _initial;
        private readonly IParser<TInput, TOutput> _right;
        private readonly LeftParser _left;

        public ApplyPostfixParser(IParser<TInput, TOutput> initial, Func<IParser<TInput, TOutput>, IParser<TInput, TOutput>> getRight)
        {
            _initial = initial;
            _left = new LeftParser();
            _right = getRight(_left);
        }

        private ApplyPostfixParser(IParser<TInput, TOutput> initial, LeftParser left, IParser<TInput, TOutput> right)
        {
            _initial = initial;
            _left = left;
            _right = right;
        }

        public IParseResult<TOutput> Parse(ISequence<TInput> t)
        {
            var result = _initial.Parse(t);
            if (!result.Success)
                return Result<TOutput>.Fail();

            var current = result.Value;
            _left.Value = result.Value;
            while (true)
            {
                var rhsResult = _right.Parse(t);
                if (!rhsResult.Success)
                    return new Result<TOutput>(true, current);

                current = rhsResult.Value;
                _left.Value = current;
            }
        }

        IParseResult<object> IParser<TInput>.ParseUntyped(ISequence<TInput> t) => (IParseResult<object>)Parse(t);

        public string Name { get; set; }

        public IParser Accept(IParserVisitorImplementation visitor) => visitor.VisitApplyPostfix(this);

        public IEnumerable<IParser> GetChildren() => new IParser[] { _initial, _right };

        public IParser ReplaceChild(IParser find, IParser replace)
        {
            if (_initial == find)
                return new ApplyPostfixParser<TInput, TOutput>(replace as IParser<TInput, TOutput>, _left, _right);
            if (_right == find)
                return new ApplyPostfixParser<TInput, TOutput>(_initial, _left, replace as IParser<TInput, TOutput>);

            return this;
        }

        public override string ToString()
        {
            var typeName = this.GetType().Name;
            return Name == null ? base.ToString() : $"{typeName} {Name}";
        }

        private class LeftParser : IParser<TInput, TOutput>
        {
            public TOutput Value { get; set; }

            public IParseResult<TOutput> Parse(ISequence<TInput> t) => new Result<TOutput>(true, Value);

            IParseResult<object> IParser<TInput>.ParseUntyped(ISequence<TInput> t) => (IParseResult<object>)Parse(t);

            public string Name { get; set; }

            public IParser Accept(IParserVisitorImplementation visitor) => this;

            public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

            public IParser ReplaceChild(IParser find, IParser replace) => this;
        }
    }
}

using System;
using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop.Parsers
{
    public class InfixOperatorParser<TOutput, TOperator> : IParser<TOutput>
        where TOutput : AstNode
        where TOperator: AstNode
    {
        private readonly IParser<TOutput> _itemParser;
        private readonly IParser<TOperator> _operatorParser;
        private readonly Func<TOutput, TOperator, TOutput, TOutput> _producer;

        public InfixOperatorParser(IParser<TOutput> itemParser, IParser<TOperator> operatorParser, Func<TOutput, TOperator, TOutput, TOutput> producer)
        {
            _itemParser = itemParser;
            _operatorParser = operatorParser;
            _producer = producer;
        }

        public TOutput TryParse(ITokenizer t)
        {
            var leftResult = _itemParser.Parse(t);
            if (!leftResult.IsSuccess)
                return null;

            var left = leftResult.GetResult();
            while (true)
            {
                var opResult = _operatorParser.Parse(t);
                if (!opResult.IsSuccess)
                    return left;
                var rightResult = _itemParser.Parse(t);
                if (!rightResult.IsSuccess)
                    return null;
                left = _producer(left, opResult.GetResult(), rightResult.GetResult());
            }
        }

        public string Name { get; set; }
        public override string ToString()
        {
            var typeName = this.GetType().Name;
            return Name == null ? base.ToString() : $"{typeName} {Name}";
        }
    }
}
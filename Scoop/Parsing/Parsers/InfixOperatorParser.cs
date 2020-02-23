using System;
using System.Collections.Generic;
using ParserObjects;
using Scoop.Parsing.Tokenization;
using Scoop.SyntaxTree;

namespace Scoop.Parsing.Parsers
{
    /// <summary>
    /// Parses a left-associative infix operator precidence level. Parses a left-hand-side production then attempts
    /// to parse an operator and then a right-hand-side production. For each successful match, it produces an
    /// operation node, sets that as the new left-hand-side and continues.
    /// </summary>
    public class InfixOperatorParser : IParser<Token, AstNode>
    {
        private readonly IParser<Token, AstNode> _left;
        private readonly IParser<Token, OperatorNode> _operatorParser;
        private readonly IParser<Token, AstNode> _right;
        private readonly Func<AstNode, OperatorNode, AstNode, AstNode> _producer;

        public InfixOperatorParser(IParser<Token, AstNode> left, IParser<Token, OperatorNode> operatorParser, IParser<Token, AstNode> right, Func<AstNode, OperatorNode, AstNode, AstNode> producer)
        {
            _left = left;
            _operatorParser = operatorParser;
            _right = right;
            _producer = producer;
        }

        public IParseResult<AstNode> Parse(ISequence<Token> t)
        {
            var result = _left.Parse(t);
            if (!result.Success)
                return new FailResult<AstNode>();
            var left = result.Value;

            while (true)
            {
                var opResult  = _operatorParser.Parse(t);
                if (!opResult.Success)
                    return new SuccessResult<AstNode>(left, result.Location);
                result = _right.Parse(t);
                var right = result.Value;
                if (!result.Success)
                {
                    var location = t.Peek()?.Location;
                    right = new EmptyNode().WithDiagnostics(location, "Missing right-hand expression for operator " + opResult.Value.Operator);
                }

                left = _producer(left, opResult.Value, right);
            }
        }

        IParseResult<object> IParser<Token>.ParseUntyped(ISequence<Token> t) => Parse(t).Untype();

        public string Name { get; set; }

        public IEnumerable<IParser> GetChildren() => new IParser[] { _left, _operatorParser, _right };

        public IParser ReplaceChild(IParser find, IParser replace)
        {
            if (_left == find)
                return new InfixOperatorParser(replace as IParser<Token, AstNode>, _operatorParser, _right, _producer);
            if (_operatorParser == find && replace is IParser<Token, OperatorNode> realOperator)
                return new InfixOperatorParser(_left, realOperator, _right, _producer);
            if (_right == find)
                return new InfixOperatorParser(_left, _operatorParser, replace as IParser<Token, AstNode>, _producer);
            return this;
        }

        public override string ToString()
        {
            var typeName = this.GetType().Name;
            return Name == null ? base.ToString() : $"{typeName} {Name}";
        }
    }
}
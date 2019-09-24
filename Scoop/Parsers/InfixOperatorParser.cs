using System;
using System.Collections.Generic;
using Scoop.Parsers.Visiting;
using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop.Parsers
{
    /// <summary>
    /// Parses a left-associative infix operator precidence level. Parses a left-hand-side production then attempts
    /// to parse an operator and then a right-hand-side production. For each successful match, it produces an
    /// operation node, sets that as the new left-hand-side and continues.
    /// </summary>
    public class InfixOperatorParser : IParser<AstNode>
    {
        private readonly IParser<AstNode> _left;
        private readonly IParser<OperatorNode> _operatorParser;
        private readonly IParser<AstNode> _right;
        private readonly Func<AstNode, OperatorNode, AstNode, AstNode> _producer;

        public InfixOperatorParser(IParser<AstNode> left, IParser<OperatorNode> operatorParser, IParser<AstNode> right, Func<AstNode, OperatorNode, AstNode, AstNode> producer)
        {
            _left = left;
            _operatorParser = operatorParser;
            _right = right;
            _producer = producer;
        }

        public AstNode TryParse(ITokenizer t)
        {
            var left = _left.TryParse(t);
            if (left == null)
                return null;

            while (true)
            {
                var op = _operatorParser.TryParse(t);
                if (op == null)
                    return left;
                var right = _right.TryParse(t) ?? new EmptyNode().WithDiagnostics(t.Peek().Location, "Missing right-hand expression for operator " + op.Operator);
                left = _producer(left, op, right);
            }
        }

        public string Name { get; set; }

        public IParser Accept(IParserVisitorImplementation visitor) => visitor.VisitInfix(this);

        public IEnumerable<IParser> GetChildren() => new[] { _left, _operatorParser, _right };

        public IParser ReplaceChild(IParser find, IParser replace)
        {
            if (_left == find)
                return new InfixOperatorParser(replace as IParser<AstNode>, _operatorParser, _right, _producer);
            if (_operatorParser == find && replace is IParser<OperatorNode> realOperator)
                return new InfixOperatorParser(_left, realOperator, _right, _producer);
            if (_right == find)
                return new InfixOperatorParser(_left, _operatorParser, replace as IParser<AstNode>, _producer);
            return this;
        }

        public override string ToString()
        {
            var typeName = this.GetType().Name;
            return Name == null ? base.ToString() : $"{typeName} {Name}";
        }
    }
}
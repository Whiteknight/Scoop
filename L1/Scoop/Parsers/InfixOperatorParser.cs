using System;
using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop.Parsers
{
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

        public override string ToString()
        {
            var typeName = this.GetType().Name;
            return Name == null ? base.ToString() : $"{typeName} {Name}";
        }
    }
}
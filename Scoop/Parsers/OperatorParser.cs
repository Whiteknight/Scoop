using System.Collections.Generic;
using System.Linq;
using Scoop.Parsers.Visiting;
using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop.Parsers
{
    /// <summary>
    /// Parse one of a list of allowable operators and return an OperatorNode
    /// </summary>
    public class OperatorParser : IParser<OperatorNode>
    {
        private readonly string[] _operators;

        public OperatorParser(params string[] operators)
        {
            _operators = operators;
        }

        public OperatorNode TryParse(ITokenizer t)
        {
            if (!t.Peek().IsOperator(_operators))
                return null;
            return new OperatorNode(t.GetNext());
        }

        public string Name { get; set; }

        public IParser Accept(IParserVisitorImplementation visitor) => visitor.VisitOperator(this);

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public IParser ReplaceChild(IParser find, IParser replace) => this;

        public override string ToString()
        {
            var ops = _operators.Length == 0 ? "<any>" : string.Join(" ", _operators);
            if (Name != null)
                return $"Operator Name={Name} Values={ops}";
            return $"Operator Values={ops}";
        }
    }
}

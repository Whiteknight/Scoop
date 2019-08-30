using NUnit.Framework;
using Scoop.SyntaxTree;
using Scoop.Tests.Utility;

namespace Scoop.Tests.Parsing
{
    [TestFixture]
    public class ParseExpressionTests
    {
        [Test]
        public void CoalesceOperator_Test()
        {
            var target = new Parser();
            var result = target.ParseExpression(@"a ?? b");
            result.Should().MatchAst(
                new InfixOperationNode
                {
                    Left = new IdentifierNode("a"),
                    Operator = new OperatorNode("??"),
                    Right = new IdentifierNode("b")
                }
            );
        }

        [Test]
        public void ConditionalOperator_Test()
        {
            var target = new Parser();
            var result = target.ParseExpression(@"true ? 0 : 1");
            result.Should().MatchAst(
                new ConditionalNode
                {
                    Condition = new KeywordNode("true"),
                    IfTrue = new IntegerNode(0),
                    IfFalse = new IntegerNode(1)
                }
            );
        }
    }
}
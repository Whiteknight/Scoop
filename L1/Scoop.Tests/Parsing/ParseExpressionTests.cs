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
    }
}
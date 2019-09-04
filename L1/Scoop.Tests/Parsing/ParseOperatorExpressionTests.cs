using NUnit.Framework;
using Scoop.SyntaxTree;
using Scoop.Tests.Utility;

namespace Scoop.Tests.Parsing
{
    [TestFixture]
    public class ParseOperatorExpressionTests
    {
        [Test]
        public void Operator_PostfixThenInfixThenPrefix()
        {
            var target = new Parser();
            var result = target.ParseExpression("x--+--y");
            result.Should().MatchAst(
                new InfixOperationNode
                {
                    Left = new PostfixOperationNode
                    {
                        Left = new IdentifierNode("x"),
                        Operator = new OperatorNode("--")
                    },
                    Operator = new OperatorNode("+"),
                    Right = new PrefixOperationNode
                    {
                        Operator = new OperatorNode("--"),
                        Right = new IdentifierNode("y")
                    }
                }
            );
        }

        [Test]
        public void AsOperator_Test()
        {
            var target = new Parser();
            var result = target.ParseExpression(@"a as b");
            result.Should().MatchAst(
                new InfixOperationNode
                {
                    Left = new IdentifierNode("a"),
                    Operator = new OperatorNode("as"),
                    Right = new TypeNode("b")
                }
            );
        }

        [Test]
        public void IsOperator_Test()
        {
            var target = new Parser();
            var result = target.ParseExpression(@"a is b");
            result.Should().MatchAst(
                new InfixOperationNode
                {
                    Left = new IdentifierNode("a"),
                    Operator = new OperatorNode("is"),
                    Right = new TypeNode("b")
                }
            );
        }

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

        [Test]
        public void LogicalAndOperator_Test()
        {
            var target = new Parser();
            var result = target.ParseExpression(@"a && b");
            result.Should().MatchAst(
                new InfixOperationNode
                {
                    Left = new IdentifierNode("a"),
                    Operator = new OperatorNode("&&"),
                    Right = new IdentifierNode("b")
                }
            );
        }

        [Test]
        public void LogicalOrOperator_Test()
        {
            var target = new Parser();
            var result = target.ParseExpression(@"a || b");
            result.Should().MatchAst(
                new InfixOperationNode
                {
                    Left = new IdentifierNode("a"),
                    Operator = new OperatorNode("||"),
                    Right = new IdentifierNode("b")
                }
            );
        }

        [Test]
        public void BitwiseAndOperator_Test()
        {
            var target = new Parser();
            var result = target.ParseExpression(@"a & b");
            result.Should().MatchAst(
                new InfixOperationNode
                {
                    Left = new IdentifierNode("a"),
                    Operator = new OperatorNode("&"),
                    Right = new IdentifierNode("b")
                }
            );
        }

        [Test]
        public void MultiplyOperator_Test()
        {
            var target = new Parser();
            var result = target.ParseExpression(@"a * b");
            result.Should().MatchAst(
                new InfixOperationNode
                {
                    Left = new IdentifierNode("a"),
                    Operator = new OperatorNode("*"),
                    Right = new IdentifierNode("b")
                }
            );
        }
    }
}
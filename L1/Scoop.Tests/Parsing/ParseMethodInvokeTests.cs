using NUnit.Framework;
using Scoop.SyntaxTree;
using Scoop.Tests.Utility;

namespace Scoop.Tests.Parsing
{
    [TestFixture]
    public class ParseMethodInvokeTests
    {
        [Test]
        public void InvokeMethod_Args()
        {
            var target = TestSuite.GetScoopGrammar();
            var result = target.ParseStatement(@"myObj.Method(1, 'b');");
            result.Should().MatchAst(
                new InvokeNode
                {
                    Instance = new MemberAccessNode
                    {
                        Instance = new IdentifierNode("myObj"),
                        MemberName = new IdentifierNode("Method"),
                        Operator = new OperatorNode(".")
                    },
                    Arguments = new ListNode<AstNode>
                    {
                        Separator = new OperatorNode(","),
                        [0] = new IntegerNode(1),
                        [1] = new CharNode('b')
                    }
                }
            );
        }

        [Test]
        public void InvokeMethod_NullInvoke()
        {
            var target = TestSuite.GetScoopGrammar();
            var result = target.ParseStatement(@"myObj?.Method();");
            result.Should().MatchAst(
                new InvokeNode
                {
                    Instance = new MemberAccessNode
                    {
                        Instance = new IdentifierNode("myObj"),
                        MemberName = new IdentifierNode("Method"),
                        Operator = new OperatorNode("?.")
                    },
                    Arguments = ListNode<AstNode>.Default()
                }
            );
        }

        [Test]
        public void InvokeMethod_NamedArgs()
        {
            var target = TestSuite.GetScoopGrammar();
            var result = target.ParseStatement(@"func(test: 1);");
            result.Should().MatchAst(
                new InvokeNode
                {
                    Instance = new IdentifierNode("func"),
                    Arguments = new ListNode<AstNode>
                    {
                        Separator = new OperatorNode(","),
                        [0] = new NamedArgumentNode
                        {
                            Name = new IdentifierNode("test"),
                            Separator = new OperatorNode(":"),
                            Value = new IntegerNode(1)
                        }
                    }
                }
            );
        }
    }
}
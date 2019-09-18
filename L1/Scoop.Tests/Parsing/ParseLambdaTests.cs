using System.Collections.Generic;
using NUnit.Framework;
using Scoop.SyntaxTree;
using Scoop.Tests.Utility;

namespace Scoop.Tests.Parsing
{
    [TestFixture]
    public class  ParseLambdaTests
    {
        [Test]
        public void Lambda_ParameterlessBodyless()
        {
            var target = TestSuite.GetScoopGrammar();
            var result = target.Expressions.Parse(@"() => {}");
            result.Should().MatchAst(
                new LambdaNode
                {
                    Parameters = ListNode<IdentifierNode>.Default(),
                    Statements = new ListNode<AstNode>()
                }
            );
        }

        [Test]
        public void Lambda_ParameterlessExpression()
        {
            var target = TestSuite.GetScoopGrammar();
            var result = target.Expressions.Parse(@"() => 5");
            result.Should().MatchAst(
                new LambdaNode
                {
                    Parameters = ListNode<IdentifierNode>.Default(),
                    Statements = new ListNode<AstNode>
                    {
                        new IntegerNode(5)
                    }
                }
            );
        }

        [Test]
        public void Lambda_OneParameter()
        {
            var target = TestSuite.GetScoopGrammar();
            var result = target.Expressions.Parse(@"a => 5");
            result.Should().MatchAst(
                new LambdaNode
                {
                    Parameters = new ListNode<IdentifierNode>
                    {
                        Separator = new OperatorNode(","),
                        [0] = new IdentifierNode("a")
                    },
                    Statements = new ListNode<AstNode>
                    {
                        new IntegerNode(5)
                    }
                }
            );
        }

        [Test]
        public void Lambda_OneParameterBody()
        {
            var target = TestSuite.GetScoopGrammar();
            var result = target.Expressions.Parse(@"a => { return 5; }");
            result.Should().MatchAst(
                new LambdaNode
                {
                    Parameters = new ListNode<IdentifierNode>
                    {
                        Separator = new OperatorNode(","),
                        [0] = new IdentifierNode("a")
                    },
                    Statements = new ListNode<AstNode>
                    {
                        new ReturnNode
                        {
                            Expression = new IntegerNode(5)
                        }
                    }
                }
            );
        }

        [Test]
        public void Lambda_Parameters()
        {
            var target = TestSuite.GetScoopGrammar();
            var result = target.Expressions.Parse(@"(a, b, c) => 5");
            result.Should().MatchAst(
                new LambdaNode
                {
                    Parameters = new ListNode<IdentifierNode>
                    {
                        Separator = new OperatorNode(","),
                        [0] = new IdentifierNode("a"),
                        [1] = new IdentifierNode("b"),
                        [2] = new IdentifierNode("c")
                    },
                    Statements = new ListNode<AstNode>
                    {
                        new IntegerNode(5)
                    }
                }
            );
        }
    }
}
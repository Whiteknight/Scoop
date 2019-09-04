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
            var target = new Parser();
            var result = target.ParseExpression(@"() => {}");
            result.Should().MatchAst(
                new LambdaNode
                {
                    Parameters = new List<AstNode>(),
                    Statements = new List<AstNode>()
                }
            );
        }

        [Test]
        public void Lambda_ParameterlessExpression()
        {
            var target = new Parser();
            var result = target.ParseExpression(@"() => 5");
            result.Should().MatchAst(
                new LambdaNode
                {
                    Parameters = new List<AstNode>(),
                    Statements = new List<AstNode>
                    {
                        new IntegerNode(5)
                    }
                }
            );
        }

        [Test]
        public void Lambda_OneParameter()
        {
            var target = new Parser();
            var result = target.ParseExpression(@"a => 5");
            result.Should().MatchAst(
                new LambdaNode
                {
                    Parameters = new List<AstNode>
                    {
                        new IdentifierNode("a")
                    },
                    Statements = new List<AstNode>
                    {
                        new IntegerNode(5)
                    }
                }
            );
        }

        [Test]
        public void Lambda_OneParameterBody()
        {
            var target = new Parser();
            var result = target.ParseExpression(@"a => { return 5; }");
            result.Should().MatchAst(
                new LambdaNode
                {
                    Parameters = new List<AstNode>
                    {
                        new IdentifierNode("a")
                    },
                    Statements = new List<AstNode>
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
            var target = new Parser();
            var result = target.ParseExpression(@"(a, b, c) => 5");
            result.Should().MatchAst(
                new LambdaNode
                {
                    Parameters = new List<AstNode>
                    {
                        new IdentifierNode("a"),
                        new IdentifierNode("b"),
                        new IdentifierNode("c")
                    },
                    Statements = new List<AstNode>
                    {
                        new IntegerNode(5)
                    }
                }
            );
        }
    }
}
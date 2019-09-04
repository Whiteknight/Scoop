using System.Collections.Generic;
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

        [Test]
        public void MemberInvoke_GenericArguments()
        {
            var target = new Parser();
            var result = target.ParseExpression(@"a.b<c>()");
            result.Should().MatchAst(
                new InvokeNode
                {
                    Instance = new MemberAccessNode
                    {
                        Instance = new IdentifierNode("a"),
                        MemberName = new IdentifierNode("b"),
                        GenericArguments = new List<AstNode>
                        {
                            new TypeNode
                            {
                                Name = new IdentifierNode("c")
                            }
                        }
                    },
                    Arguments = new List<AstNode>()
                }
            );
        }

        [Test]
        public void Member_LessThan()
        {
            var target = new Parser();
            var result = target.ParseExpression(@"a.b<c");
            result.Should().MatchAst(
                new InfixOperationNode
                {
                    Left =  new MemberAccessNode
                    {
                        Instance = new IdentifierNode("a"),
                        MemberName = new IdentifierNode("b"),
                    },
                    Operator = new OperatorNode("<"),
                    Right = new IdentifierNode("c")
                }
            );
        }

        [Test]
        public void MemberInvoke_GenericMethodChain()
        {
            var target = new Parser();
            var result = target.ParseExpression(@"e.First<List<int>>().First<int>()");
            result.Should().MatchAst(
                new InvokeNode
                {
                    Instance = new MemberAccessNode
                    {
                        Instance = new InvokeNode
                        {
                            Instance = new MemberAccessNode
                            {
                                Instance = new IdentifierNode("e"),
                                MemberName = new IdentifierNode("First"),
                                GenericArguments = new List<AstNode>
                                {
                                    new TypeNode
                                    {
                                        Name = new IdentifierNode("List"),
                                        GenericArguments = new List<AstNode>
                                        {
                                            new TypeNode
                                            {
                                                Name = new IdentifierNode("int")
                                            }
                                        }
                                    }
                                }
                            },
                            Arguments = new List<AstNode>()
                        },
                        MemberName = new IdentifierNode("First"),
                        GenericArguments = new List<AstNode> {
                            new TypeNode {
                                Name = new IdentifierNode("int")
                            }
                        }
                    },
                    Arguments = new List<AstNode>()
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
        public void String_Test()
        {
            var target = new Parser();
            var result = target.ParseExpression("\"test\"");
            result.Should().MatchAst(
                new StringNode("\"test\"")
            );
        }

        [Test]
        public void String_Block()
        {
            var target = new Parser();
            var result = target.ParseExpression("@\"test\"");
            result.Should().MatchAst(
                new StringNode("@\"test\"")
            );
        }

        [Test]
        public void String_Interpolated()
        {
            var target = new Parser();
            var result = target.ParseExpression("$\"test\"");
            result.Should().MatchAst(
                new StringNode("$\"test\"")
            );
        }

        [Test]
        public void String_Interpolated_Int()
        {
            var target = new Parser();
            var result = target.ParseExpression("$\"{5}\"");
            result.Should().MatchAst(
                new StringNode("$\"{5}\"")
            );
        }

        [Test]
        public void String_Interpolated_NestedString()
        {
            var target = new Parser();
            var result = target.ParseExpression("$\"{\"test\"}\"");
            result.Should().MatchAst(
                new StringNode("$\"{\"test\"}\"")
            );
        }

        [Test]
        public void String_Interpolated_NestedBlockString()
        {
            var target = new Parser();
            var result = target.ParseExpression("$\"{@\"test\"}\"");
            result.Should().MatchAst(
                new StringNode("$\"{@\"test\"}\"")
            );
        }

        [Test]
        public void String_Interpolated_NestedInterpolated()
        {
            var target = new Parser();
            var result = target.ParseExpression("$\"{$\"test\"}\"");
            result.Should().MatchAst(
                new StringNode("$\"{$\"test\"}\"")
            );
        }

        [Test]
        public void String_InterpolatedBlock()
        {
            var target = new Parser();
            var result = target.ParseExpression("$@\"test\"");
            result.Should().MatchAst(
                new StringNode("$@\"test\"")
            );
        }
    }
}
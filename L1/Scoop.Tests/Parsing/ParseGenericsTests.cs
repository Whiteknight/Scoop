using System.Collections.Generic;
using NUnit.Framework;
using Scoop.SyntaxTree;
using Scoop.Tests.Utility;

namespace Scoop.Tests.Parsing
{
    [TestFixture]
    public class ParseGenericsTests
    {
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

        // Test here that we don't panic seeing a less-than and think it's the start of a 
        // generic argument
        [Test]
        public void Member_LessThan()
        {
            var target = new Parser();
            var result = target.ParseExpression(@"a.b<c");
            result.Should().MatchAst(
                new InfixOperationNode
                {
                    Left = new MemberAccessNode
                    {
                        Instance = new IdentifierNode("a"),
                        MemberName = new IdentifierNode("b"),
                    },
                    Operator = new OperatorNode("<"),
                    Right = new IdentifierNode("c")
                }
            );
        }
    }
}
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
            var target = TestSuite.GetScoopGrammar();
            var result = target.ParseExpression(@"a.b<c>()");
            result.Should().MatchAst(
                new InvokeNode
                {
                    Instance = new MemberAccessNode
                    {
                        Instance = new IdentifierNode("a"),
                        MemberName = new IdentifierNode("b"),
                        GenericArguments = new ListNode<TypeNode>
                        {
                            Separator = new OperatorNode(","),
                            [0] = new TypeNode("c")
                        },
                        Operator = new OperatorNode(".")
                    },
                    Arguments = ListNode<AstNode>.Default()
                }
            );
        }

        [Test]
        public void MemberInvoke_GenericMethodChain()
        {
            var target = TestSuite.GetScoopGrammar();
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
                                GenericArguments = new ListNode<TypeNode>
                                {
                                    Separator = new OperatorNode(","),
                                    [0] = new TypeNode
                                    {
                                        Name = new IdentifierNode("List"),
                                        GenericArguments = new ListNode<TypeNode>
                                        {
                                            Separator = new OperatorNode(","),
                                            [0] = new TypeNode("int")
                                        }
                                    }
                                },
                                Operator = new OperatorNode(".")
                            },
                            Arguments = ListNode<AstNode>.Default()
                        },
                        MemberName = new IdentifierNode("First"),
                        GenericArguments = new ListNode<TypeNode>
                        {
                            Separator = new OperatorNode(","),
                            [0] = new TypeNode("int")
                        },
                        Operator = new OperatorNode(".")
                    },
                    Arguments = ListNode<AstNode>.Default()
                }
            );
        }

        // Test here that we don't panic seeing a less-than and think it's the start of a 
        // generic argument
        [Test]
        public void Member_LessThan()
        {
            var target = TestSuite.GetScoopGrammar();
            var result = target.ParseExpression(@"a.b<c");
            result.Should().MatchAst(
                new InfixOperationNode
                {
                    Left = new MemberAccessNode
                    {
                        Instance = new IdentifierNode("a"),
                        MemberName = new IdentifierNode("b"),
                        Operator = new OperatorNode(".")
                    },
                    Operator = new OperatorNode("<"),
                    Right = new IdentifierNode("c")
                }
            );
        }
    }
}
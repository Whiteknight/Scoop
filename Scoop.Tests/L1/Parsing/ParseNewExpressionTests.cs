﻿using NUnit.Framework;
using Scoop.Parsers;
using Scoop.SyntaxTree;
using Scoop.Tests.Utility;

namespace Scoop.Tests.L1.Parsing
{
    [TestFixture]
    public class ParseNewExpressionTests
    {
        [Test]
        public void New_MyClass()
        {
            var target = TestSuite.GetGrammar();
            var result = target.Statements.Parse(@"new MyClass();");
            result.Should().MatchAst(
                new NewNode
                {
                    Type = new TypeNode
                    {
                        Name = new IdentifierNode("MyClass")
                    },
                    Arguments = ListNode<AstNode>.Default()
                }
            );
        }

        [Test]
        public void New_MyClassArgs()
        {
            var target = TestSuite.GetGrammar();
            var result = target.Statements.Parse(@"new MyClass(1, ""test"");");
            result.Should().MatchAst(
                new NewNode
                {
                    Type = new TypeNode
                    {
                        Name = new IdentifierNode("MyClass")
                    },
                    Arguments = new ListNode<AstNode>
                    {
                        Separator = new OperatorNode(","),
                        [0] = new IntegerNode(1),
                        [1] = new StringNode("\"test\"")
                    }
                }
            );
        }

        [Test]
        public void New_MyClassChild()
        {
            var target = TestSuite.GetGrammar();
            var result = target.Statements.Parse(@"new MyClass.Child();");
            result.Should().MatchAst(
                new NewNode
                {
                    Type = new TypeNode
                    {
                        Name = new IdentifierNode("MyClass"),
                        Child = new TypeNode
                        {
                            Name = new IdentifierNode("Child")
                        }
                    },
                    Arguments = ListNode<AstNode>.Default()
                }
            );
        }

        [Test]
        public void New_ListOfMyClass()
        {
            var target = TestSuite.GetGrammar();
            var result = target.Statements.Parse(@"new List<MyClass>();");
            result.Should().MatchAst(
                new NewNode
                {
                    Type = new TypeNode
                    {
                        Name = new IdentifierNode("List"),
                        GenericArguments = new ListNode<TypeNode>
                        {
                            Separator = new OperatorNode(","),
                            [0] = new TypeNode("MyClass")
                        }
                    },
                    Arguments = ListNode<AstNode>.Default()
                }
            );
        }

        [Test]
        public void New_ListOfMyClassChild()
        {
            var target = TestSuite.GetGrammar();
            var result = target.Statements.Parse(@"new List<MyClass.Child>();");
            result.Should().MatchAst(
                new NewNode
                {
                    Type = new TypeNode
                    {
                        Name = new IdentifierNode("List"),
                        GenericArguments = new ListNode<TypeNode>
                        {
                            Separator = new OperatorNode(","),
                            [0] = new TypeNode
                            {
                                Name = new IdentifierNode("MyClass"),
                                Child = new TypeNode("Child")
                            }
                        }
                    },
                    Arguments = ListNode<AstNode>.Default()
                }
            );
        }

        [Test]
        public void New_ComplexType()
        {
            var target = TestSuite.GetGrammar();
            var result = target.Statements.Parse(@"new A<B>.C<D.E<F>>();");
            result.Should().MatchAst(
                new NewNode
                {
                    Arguments = ListNode<AstNode>.Default(),
                    Type = new TypeNode
                    {
                        Name = new IdentifierNode("A"),
                        GenericArguments = new ListNode<TypeNode>
                        {
                            Separator = new OperatorNode(","),
                            [0] = new TypeNode("B")
                        },
                        Child = new TypeNode
                        {
                            Name = new IdentifierNode("C"),
                            GenericArguments = new ListNode<TypeNode>
                            {
                                Separator = new OperatorNode(","),
                                [0] = new TypeNode
                                {
                                    Name = new IdentifierNode("D"),
                                    Child = new TypeNode
                                    {
                                        Name = new IdentifierNode("E"),
                                        GenericArguments = new ListNode<TypeNode>
                                        {
                                            Separator = new OperatorNode(","),
                                            [0] = new TypeNode("F")
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            );
        }

        [Test]
        public void New_AnonymousType()
        {
            var target = TestSuite.GetGrammar();
            var result = target.Statements.Parse(@"new { A = ""test"" };");
            result.Should().MatchAst(
                new NewNode
                {
                    Initializers = new ListNode<AstNode>
                    {
                        Separator = new OperatorNode(","),
                        [0] = new PropertyInitializerNode
                        {
                            Property = new IdentifierNode("A"),
                            Value = new StringNode("\"test\"")
                        }
                    }
                }
            );
        }
    }
}
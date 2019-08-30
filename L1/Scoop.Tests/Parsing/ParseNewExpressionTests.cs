using System.Collections.Generic;
using NUnit.Framework;
using Scoop.SyntaxTree;
using Scoop.Tests.Utility;

namespace Scoop.Tests.Parsing
{
    [TestFixture]
    public class ParseNewExpressionTests
    {
        [Test]
        public void New_MyClass()
        {
            var target = new Parser();
            var result = target.ParseStatement(@"new MyClass();");
            result.Should().MatchAst(
                new NewNode
                {
                    Type = new TypeNode
                    {
                        Name = new IdentifierNode("MyClass"),
                        GenericArguments = new List<AstNode>()
                    },
                    Arguments = new List<AstNode>()
                }
            );
        }

        [Test]
        public void New_MyClassArgs()
        {
            var target = new Parser();
            var result = target.ParseStatement(@"new MyClass(1, ""test"");");
            result.Should().MatchAst(
                new NewNode
                {
                    Type = new TypeNode
                    {
                        Name = new IdentifierNode("MyClass"),
                        GenericArguments = new List<AstNode>()
                    },
                    Arguments = new List<AstNode>
                    {
                        new IntegerNode(1),
                        new StringNode("test")
                    }
                }
            );
        }

        [Test]
        public void New_MyClassChild()
        {
            var target = new Parser();
            var result = target.ParseStatement(@"new MyClass.Child();");
            result.Should().MatchAst(
                new NewNode
                {
                    Type = new ChildTypeNode
                    {
                        Parent = new TypeNode
                        {
                            Name = new IdentifierNode("MyClass"),
                            GenericArguments = new List<AstNode>()
                        },
                        Child = new TypeNode
                        {
                            Name = new IdentifierNode("Child"),
                            GenericArguments = new List<AstNode>()
                        }
                    },
                    Arguments = new List<AstNode>()
                }
            );
        }

        [Test]
        public void New_ListOfMyClass()
        {
            var target = new Parser();
            var result = target.ParseStatement(@"new List<MyClass>();");
            result.Should().MatchAst(
                new NewNode
                {
                    Type = new TypeNode
                    {
                        Name = new IdentifierNode("List"),
                        GenericArguments = new List<AstNode>
                        {
                            new TypeNode
                            {
                                Name = new IdentifierNode("MyClass"),
                                GenericArguments = new List<AstNode>()
                            }
                        }
                    },
                    Arguments = new List<AstNode>()
                }
            );
        }

        [Test]
        public void New_ListOfMyClassChild()
        {
            var target = new Parser();
            var result = target.ParseStatement(@"new List<MyClass.Child>();");
            result.Should().MatchAst(
                new NewNode
                {
                    Type = new TypeNode
                    {
                        Name = new IdentifierNode("List"),
                        GenericArguments = new List<AstNode>
                        {
                            new ChildTypeNode
                            {
                                Parent = new TypeNode
                                {
                                    Name = new IdentifierNode("MyClass"),
                                    GenericArguments = new List<AstNode>()
                                },
                                Child = new TypeNode
                                {
                                    Name = new IdentifierNode("Child"),
                                    GenericArguments = new List<AstNode>()
                                }
                            }
                        }
                    },
                    Arguments = new List<AstNode>()
                }
            );
        }

        [Test]
        public void New_ComplexType()
        {
            var target = new Parser();
            var result = target.ParseStatement(@"new A<B>.C<D.E<F>>();");
            result.Should().MatchAst(
                new NewNode
                {
                    Arguments = new List<AstNode>(),
                    Type = new ChildTypeNode
                    {
                        Parent = new TypeNode
                        {
                            Name = new IdentifierNode("A"),
                            GenericArguments = new List<AstNode>
                            {
                                new TypeNode
                                {
                                    Name = new IdentifierNode("B"),
                                    GenericArguments = new List<AstNode>()
                                }
                            }
                        },
                        Child = new TypeNode
                        {
                            Name = new IdentifierNode("C"),
                            GenericArguments = new List<AstNode>
                            {
                                new ChildTypeNode
                                {
                                    Parent = new TypeNode
                                    {
                                        Name = new IdentifierNode("D"),
                                        GenericArguments = new List<AstNode>(),
                                    },
                                    Child = new TypeNode
                                    {
                                        Name = new IdentifierNode("E"),
                                        GenericArguments = new List<AstNode>
                                        {
                                            new TypeNode
                                            {
                                                Name = new IdentifierNode("F"),
                                                GenericArguments = new List<AstNode>()
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            );
        }
    }
}
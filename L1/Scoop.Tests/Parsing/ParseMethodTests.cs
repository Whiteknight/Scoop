﻿using System.Collections.Generic;
using NUnit.Framework;
using Scoop.SyntaxTree;
using Scoop.Tests.Utility;

namespace Scoop.Tests.Parsing
{
    [TestFixture]
    public class ParseMethodTests
    {
        [Test]
        public void ParseMethod_NewListOfMyClass()
        {
            var target = new Parser();
            var result = target.ParseClassMember(@"
public List<int[]> GetListOfIntArrays()
{
    return new List<int[]>();
}");
            result.Should().MatchAst(
                new MethodNode
                {
                    Name = new IdentifierNode("GetListOfIntArrays"),
                    AccessModifier = new KeywordNode("public"),
                    ReturnType = new TypeNode
                    {
                        Name = new IdentifierNode("List"),
                        GenericArguments = new List<AstNode>
                        {
                            new ArrayTypeNode
                            {
                                ElementType = new TypeNode
                                {
                                    Name = new IdentifierNode("int")
                                }
                            }
                        }
                    },
                    Parameters = new List<ParameterNode>(),
                    Statements = new List<AstNode>
                    {
                        new ReturnNode
                        {
                            Expression = new NewNode
                            {
                                Type = new TypeNode
                                {
                                    Name = new IdentifierNode("List"),
                                    GenericArguments = new List<AstNode>
                                    {
                                        new ArrayTypeNode
                                        {
                                            ElementType = new TypeNode
                                            {
                                                Name = new IdentifierNode("int")
                                            }
                                        }
                                    }
                                },
                                Arguments = new List<AstNode>()
                            }
                        }
                    }
                }
            );
        }

        [Test]
        public void ParseMethod_UsingStatementAssignment()
        {
            var target = new Parser();
            var result = target.ParseClassMember(@"
public void MyMethod()
{
    using (var x = new Disposable())
        x.DoWork();
}");
            result.Should().MatchAst(
                new MethodNode
                {
                    Name = new IdentifierNode("MyMethod"),
                    AccessModifier = new KeywordNode("public"),
                    ReturnType = new TypeNode("void"),
                    Parameters = new List<ParameterNode>(),
                    Statements = new List<AstNode>
                    {
                        new UsingStatementNode
                        {
                            Disposable = new InfixOperationNode
                            {
                                Left = new VariableDeclareNode
                                {
                                    Name = new IdentifierNode("x"),
                                },
                                Operator = new OperatorNode("="),
                                Right = new NewNode
                                {
                                    Type = new TypeNode("Disposable"),
                                    Arguments = new List<AstNode>()
                                }
                            },
                            Statement = new InvokeNode
                            {
                                Instance = new MemberAccessNode
                                {
                                    Instance = new IdentifierNode("x"),
                                    MemberName = new IdentifierNode("DoWork")
                                },
                                Arguments = new List<AstNode>()
                            }
                        }
                    }
                }
            );
        }

        [Test]
        public void ParseMethod_UsingStatementExpression()
        {
            var target = new Parser();
            var result = target.ParseClassMember(@"
public void MyMethod()
{
    using (new Disposable())
        x.DoWork();
}");
            result.Should().MatchAst(
                new MethodNode
                {
                    Name = new IdentifierNode("MyMethod"),
                    AccessModifier = new KeywordNode("public"),
                    ReturnType = new TypeNode("void"),
                    Parameters = new List<ParameterNode>(),
                    Statements = new List<AstNode>
                    {
                        new UsingStatementNode
                        {
                            Disposable = new NewNode
                            {
                                Type = new TypeNode("Disposable"),
                                Arguments = new List<AstNode>()
                            },
                            Statement = new InvokeNode
                            {
                                Instance = new MemberAccessNode
                                {
                                    Instance = new IdentifierNode("x"),
                                    MemberName = new IdentifierNode("DoWork")
                                },
                                Arguments = new List<AstNode>()
                            }
                        }
                    }
                }
            );
        }

        [Test]
        public void ParseMethod_Parameters()
        {
            var target = new Parser();
            var result = target.ParseClassMember(@"
public void TestMethod(int a, double b, string c)
{
}");
            result.Should().MatchAst(
                new MethodNode
                {
                    Name = new IdentifierNode("TestMethod"),
                    AccessModifier = new KeywordNode("public"),
                    ReturnType = new TypeNode
                    {
                        Name = new IdentifierNode("void")
                    },
                    Parameters = new List<ParameterNode>
                    {
                        new ParameterNode
                        {
                            Type = new TypeNode
                            {
                                Name = new IdentifierNode("int")
                            },
                            Name = new IdentifierNode("a")
                        },
                        new ParameterNode
                        {
                            Type = new TypeNode
                            {
                                Name = new IdentifierNode("double")
                            },
                            Name = new IdentifierNode("b")
                        },
                        new ParameterNode
                        {
                            Type = new TypeNode
                            {
                                Name = new IdentifierNode("string")
                            },
                            Name = new IdentifierNode("c")
                        }
                    },
                    Statements = new List<AstNode>()
                }
            );
        }

        [Test]
        public void ParseMethod_DefaultParameter()
        {
            var target = new Parser();
            var result = target.ParseClassMember(@"
public void TestMethod(int a = 5)
{
}");
            result.Should().MatchAst(
                new MethodNode
                {
                    Name = new IdentifierNode("TestMethod"),
                    AccessModifier = new KeywordNode("public"),
                    ReturnType = new TypeNode
                    {
                        Name = new IdentifierNode("void")
                    },
                    Parameters = new List<ParameterNode>
                    {
                        new ParameterNode
                        {
                            Type = new TypeNode
                            {
                                Name = new IdentifierNode("int")
                            },
                            Name = new IdentifierNode("a"),
                            DefaultValue = new IntegerNode(5)
                        }
                    },
                    Statements = new List<AstNode>()
                }
            );
        }

        [Test]
        public void ParseMethod_Params()
        {
            var target = new Parser();
            var result = target.ParseClassMember(@"
public void TestMethod(params int[] a)
{
}");
            result.Should().MatchAst(
                new MethodNode
                {
                    Name = new IdentifierNode("TestMethod"),
                    AccessModifier = new KeywordNode("public"),
                    ReturnType = new TypeNode("void"),
                    Parameters = new List<ParameterNode>
                    {
                        new ParameterNode
                        {
                            IsParams = true,
                            Type = new ArrayTypeNode {
                                ElementType = new TypeNode("int")
                            },
                            Name = new IdentifierNode("a")
                        }
                    },
                    Statements = new List<AstNode>()
                }
            );
        }

        [Test]
        public void ParseMethod_AsyncAwait()
        {
            var target = new Parser();
            var result = target.ParseClassMember(@"
public async Task TestMethod(Task t)
{
    await t;
}");
            result.Should().MatchAst(
                new MethodNode
                {
                    Name = new IdentifierNode("TestMethod"),
                    AccessModifier = new KeywordNode("public"),
                    Modifiers = new List<KeywordNode>
                    {
                        new KeywordNode("async")
                    },
                    ReturnType = new TypeNode("Task"),
                    Parameters = new List<ParameterNode>
                    {
                        new ParameterNode
                        {
                            Type = new TypeNode("Task"),
                            Name = new IdentifierNode("t")
                        }
                    },
                    Statements = new List<AstNode>
                    {
                        new PrefixOperationNode
                        {
                            Operator = new OperatorNode("await"),
                            Right = new IdentifierNode("t")
                        }
                    }
                }
            );
        }

        [Test]
        public void ParseMethod_Const()
        {
            var target = new Parser();
            var result = target.ParseClassMember(@"
public int TestMethod()
{
    const int myValue = 5;
    return myValue;
}");
            result.Should().MatchAst(
                new MethodNode
                {
                    AccessModifier = new KeywordNode("public"),
                    ReturnType = new TypeNode("int"),
                    Name = new IdentifierNode("TestMethod"),
                    Parameters = new List<ParameterNode>(),
                    Statements = new List<AstNode>
                    {
                        new ConstNode
                        {
                            Type = new TypeNode("int"),
                            Name = new IdentifierNode("myValue"),
                            Value = new IntegerNode(5)
                        },
                        new ReturnNode
                        {
                            Expression = new IdentifierNode("myValue")
                        }
                    }
                }
            );
        }
    }
}

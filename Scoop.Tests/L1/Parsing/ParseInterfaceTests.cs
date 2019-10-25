using System.Collections.Generic;
using NUnit.Framework;
using Scoop.Parsing;
using Scoop.SyntaxTree;
using Scoop.Tests.Utility;

namespace Scoop.Tests.L1.Parsing
{
    [TestFixture]
    public class ParseInterfaceTests
    {
        [Test]
        public void ParseInterface_Test()
        {
            var target = TestSuite.GetGrammar();
            var result = target.Interfaces.Parse("public interface MyInterface { }");
            result.Should().MatchAst(
                new InterfaceNode
                {
                    AccessModifier = new KeywordNode("public"),
                    Name = new IdentifierNode("MyInterface"),
                    Members = new ListNode<MethodDeclareNode>
                    {
                        Items = new List<MethodDeclareNode>()
                    }
                }
            );
        }

        [Test]
        public void ParseInterface_MethodDeclares()
        {
            var target = TestSuite.GetGrammar();
            var result = target.Interfaces.Parse(@"
public interface MyInterface 
{ 
    int Method1();
    string Method2();
}");
            result.Should().MatchAst(
                new InterfaceNode
                {
                    AccessModifier = new KeywordNode("public"),
                    Name = new IdentifierNode("MyInterface"),
                    Members = new ListNode<MethodDeclareNode>
                    {
                        new MethodDeclareNode
                        {
                            Name = new IdentifierNode("Method1"),
                            Parameters = ListNode<ParameterNode>.Default(),
                            ReturnType = new TypeNode
                            {
                                Name = new IdentifierNode("int")
                            }
                        },
                        new MethodDeclareNode
                        {
                            Name = new IdentifierNode("Method2"),
                            ReturnType = new TypeNode
                            {
                                Name = new IdentifierNode("string")
                            },
                            Parameters = ListNode<ParameterNode>.Default()
                        }
                    }
                }
            );
        }

        [Test]
        public void ParseInterface_MethodDeclaresParameters()
        {
            var target = TestSuite.GetGrammar();
            var result = target.Interfaces.Parse(@"
public interface MyInterface 
{ 
    int Method1(int a, double b, string c);
}");
            result.Should().MatchAst(
                new InterfaceNode
                {
                    AccessModifier = new KeywordNode("public"),
                    Name = new IdentifierNode("MyInterface"),
                    Members = new ListNode<MethodDeclareNode>
                    {
                        new MethodDeclareNode
                        {
                            Name = new IdentifierNode("Method1"),
                            ReturnType = new TypeNode
                            {
                                Name = new IdentifierNode("int")
                            },
                            Parameters = new ListNode<ParameterNode>
                            {
                                Separator = new OperatorNode(","),
                                [0] = new ParameterNode
                                {
                                    Type = new TypeNode
                                    {
                                        Name = new IdentifierNode("int")
                                    },
                                    Name = new IdentifierNode("a")
                                },
                                [1] = new ParameterNode
                                {
                                    Type = new TypeNode
                                    {
                                        Name = new IdentifierNode("double")
                                    },
                                    Name = new IdentifierNode("b")
                                },
                                [2] = new ParameterNode
                                {
                                    Type = new TypeNode
                                    {
                                        Name = new IdentifierNode("string")
                                    },
                                    Name = new IdentifierNode("c")
                                }
                            }
                        }
                    }
                }
            );
        }

        [Test]
        public void ParseInterface_GenericInterfaceAndMethod()
        {
            var target = TestSuite.GetGrammar();
            var result = target.Interfaces.Parse(@"
public interface MyInterface<TA> 
{
    TB MyMethod<TB>();
}");
            result.Should().MatchAst(
                new InterfaceNode
                {
                    AccessModifier = new KeywordNode("public"),
                    Name = new IdentifierNode("MyInterface"),
                    GenericTypeParameters = new ListNode<IdentifierNode>
                    {
                        Separator = new OperatorNode(","),
                        [0] = new IdentifierNode("TA")
                    },
                    Members = new ListNode<MethodDeclareNode>
                    {
                        new MethodDeclareNode
                        {
                            ReturnType = new TypeNode
                            {
                                Name = new IdentifierNode("TB")
                            },
                            Name = new IdentifierNode("MyMethod"),
                            GenericTypeParameters = new ListNode<IdentifierNode>
                            {
                                Separator = new OperatorNode(","),
                                [0] = new IdentifierNode("TB")
                            },
                            Parameters = ListNode<ParameterNode>.Default()
                        }
                    }
                }
            );
        }

        [Test]
        public void ParseInterface_InheritInterfaces()
        {
            var target = TestSuite.GetGrammar();
            var result = target.Interfaces.Parse("public interface MyInterface : IFaceA, IFaceB { }");
            result.Should().MatchAst(
                new InterfaceNode
                {
                    AccessModifier = new KeywordNode("public"),
                    Name = new IdentifierNode("MyInterface"),
                    Interfaces = new ListNode<TypeNode>
                    {
                        Separator = new OperatorNode(","),
                        [0] = new TypeNode
                        {
                            Name = new IdentifierNode("IFaceA")
                        },
                        [1] = new TypeNode
                        {
                            Name = new IdentifierNode("IFaceB")
                        }
                    },
                    Members = new ListNode<MethodDeclareNode>
                    {
                        Items = new List<MethodDeclareNode>()
                    }
                }
            );
        }

        [Test]
        public void ParseInterface_GenericInterfaceAndMethodConstraints()
        {
            var target = TestSuite.GetGrammar();
            var result = target.Interfaces.Parse(@"
public interface MyInterface<TA> 
    where TA : class, new()
{
    TB MyMethod<TB, TC>() 
        where TB : IMyInterface, new()
        where TC : class, IMyInterface;
}");
            result.Should().MatchAst(
                new InterfaceNode
                {
                    AccessModifier = new KeywordNode("public"),
                    Name = new IdentifierNode("MyInterface"),
                    GenericTypeParameters = new ListNode<IdentifierNode>
                    {
                        Separator = new OperatorNode(","),
                        [0] = new IdentifierNode("TA")
                    },
                    TypeConstraints = new ListNode<TypeConstraintNode>
                    {
                        new TypeConstraintNode
                        {
                            Type = new IdentifierNode("TA"),
                            Constraints = new ListNode<AstNode>
                            {
                                Separator = new OperatorNode(","),
                                [0] = new KeywordNode("class"),
                                [1] = new KeywordNode("new()")
                            }
                        }
                    },
                    Members = new ListNode<MethodDeclareNode>
                    {
                        new MethodDeclareNode
                        {
                            ReturnType = new TypeNode("TB"),
                            Name = new IdentifierNode("MyMethod"),
                            GenericTypeParameters = new ListNode<IdentifierNode>
                            {
                                Separator = new OperatorNode(","),
                                [0] = new IdentifierNode("TB"),
                                [1] = new IdentifierNode("TC")
                            },
                            Parameters = ListNode<ParameterNode>.Default(),
                            TypeConstraints = new ListNode<TypeConstraintNode>
                            {
                                new TypeConstraintNode
                                {
                                    Type = new IdentifierNode("TB"),
                                    Constraints = new ListNode<AstNode>
                                    {
                                        Separator = new OperatorNode(","),
                                        [0] = new TypeNode("IMyInterface"),
                                        [1] = new KeywordNode("new()")
                                    }
                                },
                                new TypeConstraintNode
                                {
                                    Type = new IdentifierNode("TC"),
                                    Constraints = new ListNode<AstNode>
                                    {
                                        Separator = new OperatorNode(","),
                                        [0] = new KeywordNode("class"),
                                        [1] = new TypeNode("IMyInterface")
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
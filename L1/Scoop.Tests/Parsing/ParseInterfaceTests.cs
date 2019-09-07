using System.Collections.Generic;
using NUnit.Framework;
using Scoop.SyntaxTree;
using Scoop.Tests.Utility;

namespace Scoop.Tests.Parsing
{
    [TestFixture]
    public class ParseInterfaceTests
    {
        [Test]
        public void ParseInterface_Test()
        {
            var target = new Parser();
            var result = target.ParseInterface("public interface MyInterface { }");
            result.Should().MatchAst(
                new InterfaceNode
                {
                    AccessModifier = new KeywordNode("public"),
                    Name = new IdentifierNode("MyInterface"),
                    Members = new List<AstNode>()
                }
            );
        }

        [Test]
        public void ParseInterface_MethodDeclares()
        {
            var target = new Parser();
            var result = target.ParseInterface(@"
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
                    Members = new List<AstNode>
                    {
                        new MethodDeclareNode
                        {
                            Name = new IdentifierNode("Method1"),
                            Parameters = new List<ParameterNode>(),
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
                            Parameters = new List<ParameterNode>()
                        }
                    }
                }
            );
        }

        [Test]
        public void ParseInterface_MethodDeclaresParameters()
        {
            var target = new Parser();
            var result = target.ParseInterface(@"
public interface MyInterface 
{ 
    int Method1(int a, double b, string c);
}");
            result.Should().MatchAst(
                new InterfaceNode
                {
                    AccessModifier = new KeywordNode("public"),
                    Name = new IdentifierNode("MyInterface"),
                    Members = new List<AstNode>
                    {
                        new MethodDeclareNode
                        {
                            Name = new IdentifierNode("Method1"),
                            ReturnType = new TypeNode
                            {
                                Name = new IdentifierNode("int")
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
                            }
                        }
                    }
                }
            );
        }

        [Test]
        public void ParseInterface_GenericInterfaceAndMethod()
        {
            var target = new Parser();
            var result = target.ParseInterface(@"
public interface MyInterface<TA> 
{
    TB MyMethod<TB>();
}");
            result.Should().MatchAst(
                new InterfaceNode
                {
                    AccessModifier = new KeywordNode("public"),
                    Name = new IdentifierNode("MyInterface"),
                    GenericTypeParameters = new List<AstNode>
                    {
                        new TypeNode
                        {
                            Name = new IdentifierNode("TA")
                        }
                    },
                    Members = new List<AstNode>
                    {
                        new MethodDeclareNode
                        {
                            ReturnType = new TypeNode
                            {
                                Name = new IdentifierNode("TB")
                            },
                            Name = new IdentifierNode("MyMethod"),
                            GenericTypeParameters = new List<AstNode>
                            {
                                new TypeNode
                                {
                                    Name = new IdentifierNode("TB")
                                }
                            },
                            Parameters = new List<ParameterNode>()
                        }
                    }
                }
            );
        }

        [Test]
        public void ParseInterface_InheritInterfaces()
        {
            var target = new Parser();
            var result = target.ParseInterface("public interface MyInterface : IFaceA, IFaceB { }");
            result.Should().MatchAst(
                new InterfaceNode
                {
                    AccessModifier = new KeywordNode("public"),
                    Name = new IdentifierNode("MyInterface"),
                    Interfaces = new List<AstNode>
                    {
                        new TypeNode
                        {
                            Name = new IdentifierNode("IFaceA")
                        },
                        new TypeNode
                        {
                            Name = new IdentifierNode("IFaceB")
                        }
                    },
                    Members = new List<AstNode>()
                }
            );
        }

        [Test]
        public void ParseInterface_GenericInterfaceAndMethodConstraints()
        {
            var target = new Parser();
            var result = target.ParseInterface(@"
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
                    GenericTypeParameters = new List<AstNode>
                    {
                        new TypeNode("TA")
                    },
                    TypeConstraints = new List<TypeConstraintNode>
                    {
                        new TypeConstraintNode
                        {
                            Type = new IdentifierNode("TA"),
                            Constraints = new List<AstNode>
                            {
                                new KeywordNode("class"),
                                new KeywordNode("new()")
                            }
                        }
                    },
                    Members = new List<AstNode>
                    {
                        new MethodDeclareNode
                        {
                            ReturnType = new TypeNode("TB"),
                            Name = new IdentifierNode("MyMethod"),
                            GenericTypeParameters = new List<AstNode>
                            {
                                new TypeNode("TB"),
                                new TypeNode("TC")
                            },
                            Parameters = new List<ParameterNode>(),
                            TypeConstraints = new List<TypeConstraintNode>
                            {
                                new TypeConstraintNode
                                {
                                    Type = new IdentifierNode("TB"),
                                    Constraints = new List<AstNode>
                                    {
                                        new TypeNode("IMyInterface"),
                                        new KeywordNode("new()")
                                    }
                                },
                                new TypeConstraintNode
                                {
                                    Type = new IdentifierNode("TC"),
                                    Constraints = new List<AstNode>
                                    {
                                        new KeywordNode("class"),
                                        new TypeNode("IMyInterface")
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